using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Net;
using System.IO;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using Image = System.Drawing.Image;


namespace SpotifyForms
{
    public partial class Form1 : Form
    {
        private SpotifyWebAPI _spotify;
        private readonly SpotifyLocalAPI _spotifyLocal;
        private Track _currentTrack;

        private PrivateProfile _profile;
        private List<FullTrack> _savedTracks;
        private List<SimplePlaylist> _playlists;

      

        public Form1()
        {
            InitializeComponent();
            pnlBrowse.Visible = true;
            pnlRadio.Visible = false;
            pnlPlaylists.Visible = false;
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
            pnlPlaylists.Visible = true;
            pnlBrowse.Visible = false;
            pnlMusic.Visible = false;
            pnlRadio.Visible = false;
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            pnlMusic.Visible = true;
            pnlBrowse.Visible = false;            
            pnlPlaylists.Visible = false;
            pnlRadio.Visible = false;

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            pnlBrowse.Visible = true;
            pnlMusic.Visible = false;
            pnlPlaylists.Visible = false;
            pnlRadio.Visible = false;
        }

        private void lblLogin_Click(object sender, EventArgs e)
        {
            Task.Run(() => RunAuthentication());

            lblLogin.Text = "Logged In";
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

        private void btnRadio_Click(object sender, EventArgs e)
        {
            pnlBrowse.Visible = false;
            pnlMusic.Visible = false;
            pnlPlaylists.Visible = false;
            pnlRadio.Visible = true;
        }

        private void pnlRadio_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtSearch_OnTextChange(object sender, EventArgs e)
        {
           
        }

        private async void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            await _spotifyLocal.Play();
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
             _spotifyLocal.Previous();
        }

        private void bunifuImageButton3_Click(object sender, EventArgs e)
        {
            _spotifyLocal.Skip();
        }

        public async void UpdateTrack(Track track)
        {
            _currentTrack = track;

            //advertLabel.Text = track.IsAd() ? "ADVERT" : "";
            progressBar1.Maximum = track.Length;

            if (track.IsAd())
                return; //Don't process further, maybe null values

            lblTitle.Text = track.TrackResource.Name;
            lblTitle.Tag = track.TrackResource.Uri;

            lblArtist.Text = track.ArtistResource.Name;
            lblArtist.Tag = track.ArtistResource.Uri;

            lblAlbum.Text = track.AlbumResource.Name;
            lblAlbum.Tag = track.AlbumResource.Uri;

            SpotifyUri uri = track.TrackResource.ParseUri();

            //trackInfoBox.Text = $@"Track Info - {uri.Id}";

            picAlbum.Image = await track.GetAlbumArtAsync(AlbumArtSize.Size160);
        }

        public void UpdatePlayingStatus(bool playing)
        {
            lblArtist.Text = playing.ToString();
        }

        private void _spotify_OnVolumeChange(object sender, VolumeChangeEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _spotify_OnVolumeChange(sender, e)));
                return;
            }
            //volumeLabel.Text = (e.NewVolume * 100).ToString(CultureInfo.InvariantCulture);
        }

        private void _spotify_OnTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _spotify_OnTrackTimeChange(sender, e)));
                return;
            }
            lblTime.Text = $@"{FormatTime(e.TrackTime)}/{FormatTime(_currentTrack.Length)}";
            if (e.TrackTime < _currentTrack.Length)
                progressTime2.Value = (int)e.TrackTime;
        }

        private static String FormatTime(double sec)
        {
            TimeSpan span = TimeSpan.FromSeconds(sec);
            String secs = span.Seconds.ToString(), mins = span.Minutes.ToString();
            if (secs.Length < 2)
                secs = "0" + secs;
            return mins + ":" + secs;
        }

        private void _spotify_OnTrackChange(object sender, TrackChangeEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _spotify_OnTrackChange(sender, e)));
                return;
            }
            UpdateTrack(e.NewTrack);
        }

        private void _spotify_OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _spotify_OnPlayStateChange(sender, e)));
                return;
            }
            UpdatePlayingStatus(e.Playing);
        }

        private void bunifuCustomLabel3_Click(object sender, EventArgs e)
        {

        }
    }
}
