using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace SpotifyForms
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            pnlBrowse.Visible = true;
            pnlMusic.Visible = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to quit?", "Exit?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.Yes)
            {
                Application.Exit();
            }            
        }

        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {

        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            pnlMusic.Visible = true;

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            pnlBrowse.Visible = true;
            pnlMusic.Visible = false;
        }
    }
}
