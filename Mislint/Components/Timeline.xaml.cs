using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mislint.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Mislint.Components
{
    public sealed partial class Timeline
    {
        private string _lastId;
        private bool _loading;
        private bool _last;
        public TimelineTypeEnum TimelineType { get; set; }
        public string UserId { get; set; }

        public Timeline()
        {
            this.InitializeComponent();
        }

        public void Reload()
        {
            this.Notes.Children.Clear();
            this._lastId = null;
            LoadTimeline();
        }

        private async Task<List<Misharp.Model.Note>> GetTimeline(TimelineTypeEnum timelineType,
            int limit = 10, string sinceId = null, string untilId = null,
            int? sinceDate = null, int? untilDate = null,
            bool allowPartial = false, bool includeMyRenotes = true,
            bool includeRenotedMyNotes = true, bool includeLocalRenotes = true,
            bool withFiles = false, bool withRenotes = true, bool withReplies = true, bool withChannelNotes = false, int reccuring = 0)
        {
            if (this.TimelineType == TimelineTypeEnum.User && this.UserId.Length == 0) throw new System.Exception("UserId is required for User timeline");
            try
            {
                var res = this.TimelineType switch
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
                if (res.Count < limit) this._last = true;
                return res;
            }
            catch
            {
                if (reccuring < 3)
                {
                    return await GetTimeline(this.TimelineType, limit, sinceId, untilId, sinceDate, untilDate, allowPartial, includeMyRenotes, includeRenotedMyNotes, includeLocalRenotes, withFiles, withRenotes, withReplies, withChannelNotes, reccuring + 1);
                }
                throw;
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

        public async void LoadTimeline()
        {
            this._loading = true;
            this.ProgressRing.Visibility = Visibility.Visible;
            var tl = (await GetTimeline(this.TimelineType, limit: 20, untilId: this._lastId));
            foreach (var note in tl)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var noteComponent = new Note(note);
                    this.Notes.Children.Add(noteComponent);
                });
            }
            this._lastId = tl[^1].Id;
            this.ProgressRing.Visibility = Visibility.Collapsed;
            this._loading = false;
        }

        private void Timeline_Loaded(object sender, RoutedEventArgs e)
        {
            //await LoadTimeline();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;
            if (!(Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 1)) return; // on bottom
            if (!this._loading && !this._last) this.LoadTimeline();
        }
    }
}
