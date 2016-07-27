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

using Contralto.IO;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Contralto.UI
{
    public partial class SystemOptions : Form
    {
        public SystemOptions()
        {
            InitializeComponent();

            PopulateUI();
        }


        /// <summary>
        /// Populates the UI fields with data from the current configuration.
        /// </summary>
        private void PopulateUI()
        {
            _selectedSystemType = Configuration.SystemType;            
            _selectedInterfaceType = Configuration.HostPacketInterfaceType;

            switch(Configuration.SystemType)
            {
                case SystemType.AltoI:
                    AltoI1KROMRadioButton.Checked = true;
                    break;

                case SystemType.OneKRom:
                    AltoII1KROMRadioButton.Checked = true;
                    break;

                case SystemType.TwoKRom:
                    AltoII2KROMRadioButton.Checked = true;
                    break;

                case SystemType.ThreeKRam:
                    AltoII3KRAMRadioButton.Checked = true;
                    break;
            }

            InterlaceDisplayCheckBox.Checked = Configuration.InterlaceDisplay;
            ThrottleSpeedCheckBox.Checked = Configuration.ThrottleSpeed;

            AltoEthernetAddressTextBox.Text = Conversion.ToOctal(Configuration.HostAddress);

            if (!Configuration.HostRawEthernetInterfacesAvailable)
            {
                // If PCAP isn't installed, the RAW Ethernet option is not available.           
                RawEthernetRadioButton.Enabled = false;

                // Ensure the option isn't set in the configuration.
                if (Configuration.HostPacketInterfaceType == PacketInterfaceType.EthernetEncapsulation)
                {
                    Configuration.HostPacketInterfaceType = PacketInterfaceType.None;
                }
            }
            
            switch(Configuration.HostPacketInterfaceType)
            {
                case PacketInterfaceType.UDPEncapsulation:
                    UDPRadioButton.Checked = true;
                    break;

                case PacketInterfaceType.EthernetEncapsulation:
                    RawEthernetRadioButton.Checked = true;
                    break;

                case PacketInterfaceType.None:
                    NoEncapsulationRadioButton.Checked = true;
                    break;
            }                                                    

            PopulateNetworkAdapterList(Configuration.HostPacketInterfaceType);                       
        }

        private void PopulateNetworkAdapterList(PacketInterfaceType encapType)
        {
            //
            // Populate the list with the interfaces available on the machine, for the
            // type of encapsulation being used.
            //                        
            HostInterfaceGroupBox.Enabled = encapType != PacketInterfaceType.None;

            EthernetInterfaceListBox.Items.Clear();

            
            // Add the "Use no interface" option
            EthernetInterfaceListBox.Items.Add(
                new EthernetInterface("None", "No network adapter"));

            
            switch (encapType)
            {
                // For UDP we show all interfaces that support IPV4, for Raw Ethernet we show only Ethernet interfaces.
                case PacketInterfaceType.UDPEncapsulation:
                    {
                        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                        foreach (NetworkInterface iface in interfaces)
                        {
                            if (iface.Supports(NetworkInterfaceComponent.IPv4))
                            {
                                EthernetInterfaceListBox.Items.Add(new EthernetInterface(iface.Name, iface.Description));
                            }
                        }
                    }
                    break;

                // Add all interfaces that PCAP knows about.
                case PacketInterfaceType.EthernetEncapsulation:
                    {
                        foreach (LivePacketDevice device in LivePacketDevice.AllLocalMachine)
                        {                            
                            EthernetInterfaceListBox.Items.Add(new EthernetInterface(device.GetNetworkInterface().Name, device.GetNetworkInterface().Description));                                                            
                        }                        
                    }                    
                    break;

                case PacketInterfaceType.None:
                    // Add nothing.
                    break;
            }            
            

            //
            // Select the one that is already selected (if any)
            //            
            EthernetInterfaceListBox.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(Configuration.HostPacketInterfaceName))
            {
                for (int i = 0; i < EthernetInterfaceListBox.Items.Count; i++)
                {
                    EthernetInterface iface = (EthernetInterface)EthernetInterfaceListBox.Items[i];

                    if (iface.Name == Configuration.HostPacketInterfaceName)
                    {
                        EthernetInterfaceListBox.SelectedIndex = i;
                        break;
                    }
                }  
            }            
        }

        private void OnSystemTypeCheckChanged(object sender, EventArgs e)
        {
            if (AltoI1KROMRadioButton.Checked)
            {
                _selectedSystemType = SystemType.AltoI;
            }
            else if (AltoII1KROMRadioButton.Checked)
            {
                _selectedSystemType = SystemType.OneKRom;
            }
            else if (AltoII2KROMRadioButton.Checked)
            {
                _selectedSystemType = SystemType.TwoKRom;
            }
            else if (AltoII3KRAMRadioButton.Checked)
            {
                _selectedSystemType = SystemType.ThreeKRam;
            }
        }

        private void OnEthernetTypeCheckedChanged(object sender, EventArgs e)
        {
            if (UDPRadioButton.Checked)
            {
                _selectedInterfaceType = PacketInterfaceType.UDPEncapsulation;
            }
            else if (RawEthernetRadioButton.Checked)
            {
                _selectedInterfaceType = PacketInterfaceType.EthernetEncapsulation;
            }
            else
            {
                _selectedInterfaceType = PacketInterfaceType.None;
            }

            PopulateNetworkAdapterList(_selectedInterfaceType);
        }

        private void OKButton_Click(object sender, EventArgs e)
        { 
            try
            {
                int testValue = Convert.ToByte(AltoEthernetAddressTextBox.Text, 8);

                if (testValue < 1 || testValue > 255)
                {
                    throw new ArgumentOutOfRangeException("Invalid host address.");
                }
            } 
            catch
            {
                MessageBox.Show("The Alto Ethernet address must be an octal value between 1 and 376.");
                return;
            }

            //
            // Commit changes back to Configuration.
            //

            EthernetInterface iface = (EthernetInterface)EthernetInterfaceListBox.SelectedItem;

            //
            // First warn the user of changes that require a restart.
            //
            if ((!(String.IsNullOrEmpty(Configuration.HostPacketInterfaceName) && EthernetInterfaceListBox.SelectedIndex == 0) &&
                    (Configuration.HostPacketInterfaceName != iface.Name ||
                     Configuration.HostPacketInterfaceType != _selectedInterfaceType)) ||
                Configuration.SystemType != _selectedSystemType)
            {
                MessageBox.Show("Changes to CPU or host Ethernet configuration will not take effect until ContrAlto is restarted.");
            }

            // System
            Configuration.SystemType = _selectedSystemType;

            // Ethernet
            Configuration.HostAddress = Convert.ToByte(AltoEthernetAddressTextBox.Text, 8);
            Configuration.HostPacketInterfaceName = iface.Name;
            Configuration.HostPacketInterfaceType = _selectedInterfaceType;

            // Display
            Configuration.InterlaceDisplay = InterlaceDisplayCheckBox.Checked;
            Configuration.ThrottleSpeed = ThrottleSpeedCheckBox.Checked;            

            this.Close();

        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private PacketInterfaceType _selectedInterfaceType;
        private SystemType _selectedSystemType;


    }
}
