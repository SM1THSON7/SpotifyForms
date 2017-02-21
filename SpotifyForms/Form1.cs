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
using System.Net;
using System.IO;
using Image = System.Drawing.Image;


namespace SpotifyForms
{
    public partial class Form1 : Form
    {
        private SpotifyWebAPI _spotify;

        private PrivateProfile _profile;
        private List<FullTrack> _savedTracks;
        private List<SimplePlaylist> _playlists;

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

        private void lblLogin_Click(object sender, EventArgs e)
        {
            Task.Run(() => RunAuthentication());
        }

        private async void RunAuthentication()
        {
            WebAPIFactory webApiFactory = new WebAPIFactory(
                "http://localhost",
                8000,
                "26d287105e31491889f3cd293d85bfea",
                Scope.UserReadPrivate | Scope.UserReadEmail | Scope.PlaylistReadPrivate | Scope.UserLibraryRead |
                Scope.UserReadPrivate | Scope.UserFollowRead | Scope.UserReadBirthdate | Scope.UserTopRead | Scope.PlaylistReadCollaborative);

            try
            {
                _spotify = await webApiFactory.GetWebApi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (_spotify == null)
                return;

            InitialSetup();
        }

        private async void InitialSetup()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(InitialSetup));
                return;
            }

            lblLogin.Enabled = false;
            _profile = _spotify.GetPrivateProfile();

            _savedTracks = GetSavedTracks();
            savedTracksCountLabel.Text = _savedTracks.Count.ToString();
            _savedTracks.ForEach(track => lstMusic.Items.Add(new ListViewItem()
            {
                Text = track.Name,
                SubItems = { string.Join(",", track.Artists.Select(source => source.Name)), track.Album.Name }
            }));

            _playlists = GetPlaylists();
            playlistsCountLabel.Text = _playlists.Count.ToString();
            _playlists.ForEach(playlist => playlistsListBox.Items.Add(playlist.Name));

            displayNameLabel.Text = _profile.DisplayName;
            countryLabel.Text = _profile.Country;
            emailLabel.Text = _profile.Email;
            accountLabel.Text = _profile.Product;

            if (_profile.Images != null && _profile.Images.Count > 0)
            {
                using (WebClient wc = new WebClient())
                {
                    byte[] imageBytes = await wc.DownloadDataTaskAsync(new Uri(_profile.Images[0].Url));
                    using (MemoryStream stream = new MemoryStream(imageBytes))
                        avatarPictureBox.Image = Image.FromStream(stream);
                }
            }
        }

        private List<FullTrack> GetSavedTracks()
        {
            Paging<SavedTrack> savedTracks = _spotify.GetSavedTracks();
            List<FullTrack> list = savedTracks.Items.Select(track => track.Track).ToList();

            while (savedTracks.Next != null)
            {
                savedTracks = _spotify.GetSavedTracks(20, savedTracks.Offset + savedTracks.Limit);
                list.AddRange(savedTracks.Items.Select(track => track.Track));
            }

            return list;
        }

        private List<SimplePlaylist> GetPlaylists()
        {
            Paging<SimplePlaylist> playlists = _spotify.GetUserPlaylists(_profile.Id);
            List<SimplePlaylist> list = playlists.Items.ToList();

            while (playlists.Next != null)
            {
                playlists = _spotify.GetUserPlaylists(_profile.Id, 20, playlists.Offset + playlists.Limit);
                list.AddRange(playlists.Items);
            }

            return list;
        }


    }
}
