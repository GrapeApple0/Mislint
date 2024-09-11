using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mislint.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Mislint.Components
{
    public sealed partial class Timeline : UserControl
    {
        private string lastId = null;
        private bool loading = false;
        public TimelineTypeEnum TimelineType { get; set; }
        public string UserId { get; set; }

        public Timeline()
        {
            this.InitializeComponent();
        }

        public async void Reload()
        {
            this.notes.Children.Clear();
            this.lastId = null;
            await LoadTimeline();
        }

        public async Task<List<Misharp.Model.Note>> GetTimeline(TimelineTypeEnum timelineType,
            int limit = 10, string sinceId = null, string untilId = null,
            int? sinceDate = null, int? untilDate = null,
            bool allowPartial = false, bool includeMyRenotes = true,
            bool includeRenotedMyNotes = true, bool includeLocalRenotes = true,
            bool withFiles = false, bool withRenotes = true, bool withReplies = true, bool withChannelNotes = false)
        {
            if (this.TimelineType == TimelineTypeEnum.User && this.UserId.Length == 0) throw new System.Exception("UserId is required for User timeline");
            try
            {
                return this.TimelineType switch
                {
                    TimelineTypeEnum.Home => (await Shared.MisharpApp.NotesApi.Timeline(limit, sinceId, untilId,
                                            sinceDate, untilDate,
                                            allowPartial, includeMyRenotes,
                                            includeRenotedMyNotes, includeLocalRenotes,
                                            withFiles, withRenotes)).Result,
                    TimelineTypeEnum.Local => (await Shared.MisharpApp.NotesApi.LocalTimeline(withFiles, withRenotes, withReplies,
                                            limit, sinceId, untilId, allowPartial, sinceDate, untilDate)).Result,
                    TimelineTypeEnum.Hybrid => (await Shared.MisharpApp.NotesApi.HybridTimeline(limit, sinceId, untilId,
                                            sinceDate, untilDate,
                                            allowPartial, includeMyRenotes,
                                            includeRenotedMyNotes, includeLocalRenotes,
                                            withFiles, withRenotes, withReplies)).Result,
                    TimelineTypeEnum.Global => (await Shared.MisharpApp.NotesApi.GlobalTimeline(withFiles, withRenotes,
                                            limit, sinceId, untilId, sinceDate, untilDate)).Result,
                    TimelineTypeEnum.User => (await Shared.MisharpApp.UsersApi.Notes(UserId, withReplies, withRenotes, withChannelNotes,
                                            limit, sinceId, untilId, sinceDate, untilDate, allowPartial, withFiles)).Result,
                    _ => (await Shared.MisharpApp.NotesApi.Timeline(limit, sinceId, untilId,
                                            sinceDate, untilDate,
                                            allowPartial, includeMyRenotes,
                                            includeRenotedMyNotes, includeLocalRenotes,
                                            withFiles, withRenotes)).Result,
                };
            }
            catch
            {
                return await GetTimeline(timelineType,
                    limit = 10, sinceId = null, untilId = null,
                    sinceDate = null, untilDate = null,
                    allowPartial = false, includeMyRenotes = true,
                    includeRenotedMyNotes = true, includeLocalRenotes = true,
                    withFiles = false, withRenotes = true, withReplies = true, withChannelNotes = false);
            }
        }

        public enum TimelineTypeEnum
        {
            Home,
            Local,
            Hybrid,
            Global,
            User,
        }

        private async Task LoadTimeline()
        {
            this.loading = true;
            this.progressRing.Visibility = Visibility.Visible;
            var tl = (await GetTimeline(this.TimelineType, limit: 20, untilId: lastId));
            foreach (var note in tl)
            {
                await Task.Run(() =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var noteComponent = new Note(note);
                        this.notes.Children.Add(noteComponent);
                    });
                });
            }
            lastId = tl[^1].Id;
            this.progressRing.Visibility = Visibility.Collapsed;
            this.loading = false;
        }

        private void Timeline_Loaded(object sender, RoutedEventArgs e)
        {
            //await LoadTimeline();
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                //if (scrollViewer.VerticalOffset == 0) // on top
                //{
                //    Debug.WriteLine("on top");
                //}
                //else
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight) // on bottom
                {
                    if (!this.loading)  await LoadTimeline();
                }
            }
        }
    }
}
