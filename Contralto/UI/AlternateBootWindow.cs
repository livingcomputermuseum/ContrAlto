using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Contralto
{
    public partial class AlternateBootOptions : Form
    {
        public AlternateBootOptions()
        {
            InitializeComponent();            
                        
            LoadBootEntries();

            if (Configuration.AlternateBootType == AlternateBootType.Disk)
            {
                DiskBootRadioButton.Checked = true;
            }
            else
            {
                EthernetBootRadioButton.Checked = true;
            }

            SetBootAddress(Configuration.BootAddress);
            SelectBootFile(Configuration.BootFile);
        }        

        private void LoadBootEntries()
        {
            foreach(BootFileEntry e in _bootEntries)
            {
                BootFileComboBox.Items.Add(e);
            }
        }

        private void SelectBootFile(ushort fileNumber)
        {
            // Find the matching entry, if any.
            bool found = false;
            for (int i = 0; i < BootFileComboBox.Items.Count; i++)
            {
                if (((BootFileEntry)BootFileComboBox.Items[i]).FileNumber == fileNumber)
                {
                    BootFileComboBox.Select(i, 1);
                    BootFileComboBox.Text = BootFileComboBox.Items[i].ToString();
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // No matching entry, just fill in the text box with the number.
                BootFileComboBox.Text = Conversion.ToOctal(fileNumber);
            }
        }       

        private void SetBootAddress(ushort address)
        {
            DiskBootAddressTextBox.Text = Conversion.ToOctal(address);
        }

        private void BootFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedBootFile = ((BootFileEntry)BootFileComboBox.SelectedItem).FileNumber;
        }        

        private void OKButton_Click(object sender, EventArgs e)
        {
            try
            {
                _selectedBootAddress = Convert.ToUInt16(DiskBootAddressTextBox.Text, 8);
            }
            catch
            {
                MessageBox.Show("The disk boot address must be an octal value between 0 and 177777.");
                return;
            }            

            if (BootFileComboBox.SelectedItem == null)
            {
                try
                {
                    _selectedBootFile = Convert.ToUInt16(BootFileComboBox.Text, 8);
                }
                catch
                {
                    MessageBox.Show("Please select a valid boot entry or type in a valid boot number.", "Invalid selection");
                    return;
                }
            }
            else
            {
                _selectedBootFile = ((BootFileEntry)BootFileComboBox.SelectedItem).FileNumber;
            }
            
            Configuration.BootAddress = _selectedBootAddress;
            Configuration.BootFile = _selectedBootFile;

            if (DiskBootRadioButton.Checked)
            {            
                Configuration.AlternateBootType = AlternateBootType.Disk;
            }
            else
            {                               
                Configuration.AlternateBootType = AlternateBootType.Ethernet;
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }      

        private BootFileEntry[] _bootEntries = new BootFileEntry[]
        {
            new BootFileEntry(0, "DMT"),
            new BootFileEntry(1, "NewOS"),
            new BootFileEntry(2, "FTP"),
            new BootFileEntry(3, "Scavenger"),
            new BootFileEntry(4, "CopyDisk"),
            new BootFileEntry(5, "CRTTest"),
            new BootFileEntry(6, "MADTest"),
            new BootFileEntry(7, "Chat"),
            new BootFileEntry(8, "NetExec"),
            new BootFileEntry(9, "PupTest"),
            new BootFileEntry(10, "EtherWatch"),
            new BootFileEntry(11, "KeyTest"),
            new BootFileEntry(13, "DiEx"),
            new BootFileEntry(15, "EDP"),
            new BootFileEntry(16, "BFSTest"),
            new BootFileEntry(17, "GateControl"),
            new BootFileEntry(18, "EtherLoad"),
        };

       

        private ushort _selectedBootFile;
        private ushort _selectedBootAddress;

    }

    public struct BootFileEntry
    {
        public BootFileEntry(ushort number, string desc)
        {
            FileNumber = number;
            Description = desc;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Conversion.ToOctal(FileNumber), Description);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public ushort FileNumber;
        public string Description;
    }
}
