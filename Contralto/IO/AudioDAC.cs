/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

using Contralto.CPU;
using Contralto.Logging;
using Contralto.Memory;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Contralto.IO
{
    /// <summary>
    /// Implements the hardware for the Audio DAC used by Ted Kaehler's
    /// ST-74 Music System (either the FM-synthesis "TWANG" system or the
    /// Sampling system.)    
    /// </summary>
    public class AudioDAC : IMemoryMappedDevice
    {
        public AudioDAC(AltoSystem system)
        {
            _system = system;
            _dacOutput = new Queue<ushort>(16384);
        }

        public void Shutdown()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
            }

            if (_waveFile != null)
            {
                _waveFile.Close();
            }
        }

        /// <summary>
        /// Comments in the FM synthesis microcode indicate:
        /// "240 SAMPLES = 18 msec"
        /// Which works out to about 13.3Khz.
        /// 
        /// Unsure if this value also applies to the Sampling microcode, but
        /// it sounds about right in action.
        /// </summary>
        public static readonly int AudioDACSamplingRate = 13000;

        /// <summary>
        /// Reads a word from the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="extendedMemory"></param>
        /// <returns></returns>
        public ushort Read(int address, TaskType task, bool extendedMemory)
        {            
            // The DAC is, as far as I can tell, write-only.
            return 0;
        }

        /// <summary>
        /// Writes a word to the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public void Load(int address, ushort data, TaskType task, bool extendedMemory)
        {
            if (Configuration.EnableAudioDAC)
            {               
                // Ensure we have a sink for audio output capture if so configured.
                if (Configuration.EnableAudioDACCapture && _waveFile == null)
                {
                    string outputFile = Path.Combine(
                        Configuration.AudioDACCapturePath,
                        string.Format("AltoAudio-{0}.wav", DateTime.Now.ToString("yyyyMMdd-hhmmss")));

                    try
                    {
                        _waveFile = new WaveFileWriter(outputFile, new WaveFormat(AudioDAC.AudioDACSamplingRate, 1));
                    }
                    catch (Exception e)
                    {
                        Log.Write(LogType.Error,
                            LogComponent.DAC,
                            "Failed to create DAC output file {0}.  Error: {1}", outputFile, e.Message);
                    }
                }

                // Ensure we have something to generate audio output with.
                if (_waveOut == null)
                {
                    _waveOut = new WaveOut();
                    _dacLock = new ReaderWriterLockSlim();
                    _waveOut.Init(new DACOutputWaveProvider(_dacOutput, _waveFile, _dacLock));
                    _waveOut.Play();
                }

                //
                // Enter the Write lock to ensure consistency with the
                // consumer (the DACOutputWaveProvider).
                //
                _dacLock.EnterWriteLock();
                _dacOutput.Enqueue(data);
                _dacLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Specifies the range (or ranges) of addresses decoded by this device.
        /// </summary>
        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }

        /// <summary>
        /// From: http://bitsavers.org/pdf/xerox/alto/memos_1975/Reserved_Alto_Memory_Locations_Jan75.pdf
        ///         
        /// #177776: Digital-Analog Converter (DAC Hardware - Kaehler)
        /// </summary>
        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0xfffe, 0xfffe),
        };
        
        private Queue<ushort> _dacOutput;
        private ReaderWriterLockSlim _dacLock;

        private AltoSystem _system;        

        private WaveOut _waveOut;
       
        private WaveFileWriter _waveFile;
    }

    //
    // Basic implementation of the NAudio WaveProvider32 class.
    // on a Read, we fill the DAC with the samples the Alto has been generated.
    // If it hasn't generated enough (it's fallen behind) we'll pad it to try
    // and reduce popping/stuttering.
    //
    // It will also flush the output to a WaveFileWriter if it has been enabled in
    // the configuration.
    //
    public class DACOutputWaveProvider : WaveProvider32
    {
        public DACOutputWaveProvider(Queue<ushort> dacOutput, WaveFileWriter waveFile, ReaderWriterLockSlim dacLock)
        {
            _dacOutput = dacOutput;
            _waveFile = waveFile;
            _dacLock = dacLock;
            SetWaveFormat(AudioDAC.AudioDACSamplingRate, 1);
            _lastSample = 0;
        }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int outSamples = 0;

            _dacLock.EnterReadLock();
            while(_dacOutput.Count > 0 && outSamples < sampleCount)
            {
                float sample = (float)_dacOutput.Dequeue() / 32768.0f - 1.0f;
                buffer[offset + outSamples] = sample;
                outSamples++;
                _lastSample = sample;
            }
            _dacLock.ExitReadLock();

            // Commit the Alto-generated samples to disk if we're saving them.
            if (_waveFile != null)
            {
                _waveFile.WriteSamples(buffer, offset, outSamples);
            }

            //
            // If we didn't have enough samples to fill the requested buffer,
            // This means the Alto has fallen behind; pad the remaining buffer with the
            // last written sample.
            //
            for (; outSamples < sampleCount; outSamples++)
            {
                buffer[offset + outSamples] = _lastSample;
            }

            return sampleCount;
        }

        /// <summary>
        /// Queue containing the samples generated by the Alto.
        /// We pull them off as WaveOut requests audio data.
        /// </summary>
        private Queue<ushort> _dacOutput;

        /// <summary>
        /// The last sample written.  Used to pad the buffer if
        /// the Alto falls behind.
        /// </summary>
        private float _lastSample;

        /// <summary>
        /// Used to ensure thread-safety of the _dacOutput queue
        /// between the Alto emulation and the WaveOut thread.
        /// </summary>
        private ReaderWriterLockSlim _dacLock;

        /// <summary>
        /// Used to write the output to a WAV file, if the option
        /// is enabled.
        /// </summary>
        private WaveFileWriter _waveFile;
    }

}
