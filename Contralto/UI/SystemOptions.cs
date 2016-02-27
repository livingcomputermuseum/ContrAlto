using Contralto.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
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
                UDPRadioButton.Checked = true;
            }
            else
            {
                if (Configuration.HostPacketInterfaceType == PacketInterfaceType.UDPEncapsulation)
                {
                    UDPRadioButton.Checked = true;
                }
                else
                {
                    RawEthernetRadioButton.Checked = true;
                }
            }

            PopulateNetworkAdapterList(UDPRadioButton.Checked);                       
        }

        private void PopulateNetworkAdapterList(bool udpEncapsulation)
        {
            //
            // Populate the list with the interfaces available on the machine, for the
            // type of encapsulation being used.
            //            
            EthernetInterfaceListBox.Items.Clear();

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Add the "Use no interface" option
            EthernetInterfaceListBox.Items.Add(
                new EthernetInterface("None", "No network adapter"));

            foreach (NetworkInterface iface in interfaces)
            {
                // For UDP we show all interfaces that support IPV4, for Raw Ethernet we show only Ethernet interfaces.
                if (udpEncapsulation)
                {
                    if (iface.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        EthernetInterfaceListBox.Items.Add(new EthernetInterface(iface.Name, iface.Description));
                    }
                }
                else 
                {
                    if (iface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        EthernetInterfaceListBox.Items.Add(new EthernetInterface(iface.Name, iface.Description));
                    }
                }
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

                    if (iface.Description.ToLowerInvariant() == Configuration.HostPacketInterfaceName.ToLowerInvariant())
                    {
                        EthernetInterfaceListBox.SelectedIndex = i;
                        break;
                    }
                }  
            }            
        }

        private void OnSystemTypeCheckChanged(object sender, EventArgs e)
        {
            if (AltoII1KROMRadioButton.Checked)
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
            else
            {
                _selectedInterfaceType = PacketInterfaceType.EthernetEncapsulation;
            }

            PopulateNetworkAdapterList(UDPRadioButton.Checked);
        }

        private void OKButton_Click(object sender, EventArgs e)
        { 
            try
            {
                int testValue = Convert.ToByte(AltoEthernetAddressTextBox.Text, 8);

                if (testValue < 1 || testValue > 254)
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
            if (Configuration.HostPacketInterfaceName.ToLowerInvariant() != iface.Description.ToLowerInvariant() ||
                Configuration.HostPacketInterfaceType != _selectedInterfaceType ||
                Configuration.SystemType != _selectedSystemType)
            {
                MessageBox.Show("Changes to CPU or Ethernet configuration will not take effect until ContrAlto is restarted.");
            }

            //System
            Configuration.SystemType = _selectedSystemType;

            // Ethernet
            Configuration.HostAddress = Convert.ToByte(AltoEthernetAddressTextBox.Text, 8);
            Configuration.HostPacketInterfaceName = iface.Description;
            Configuration.HostPacketInterfaceType = _selectedInterfaceType;

            // Display
            Configuration.InterlaceDisplay = InterlaceDisplayCheckBox.Checked;
            Configuration.ThrottleSpeed = ThrottleSpeedCheckBox.Checked;

            Configuration.WriteConfiguration();

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
