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

using System;
using System.Reflection;
using System.Windows.Forms;

namespace Contralto
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();

            VersionLabel.Text += typeof(Program).Assembly.GetName().Version;            
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:joshd@livingcomputers.org");
        }

        private void OnSiteLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.livingcomputers.org");
        }
    }
}
