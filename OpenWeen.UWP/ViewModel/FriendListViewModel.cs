﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenWeen.Core.Model.User;

namespace OpenWeen.UWP.ViewModel
{
    public class FriendListViewModel : UserListViewModelBase
    {
        public FriendListViewModel(long uid) : base(uid, "关注")
        {
        }

        protected override async Task<IEnumerable<UserModel>> LoadMoreOverride()
        {
            var item = await Core.Api.Friendships.Friends.GetFriends(UID, cursor: _pageCount);
            _pageCount = int.Parse(item.NextCursor);
            return item.Users;
        }

        protected override async Task<Tuple<int, List<UserModel>>> RefreshOverride()
        {
            var item = await Core.Api.Friendships.Friends.GetFriends(UID, cursor: _pageCount);
            _pageCount = int.Parse(item.NextCursor);
            return Tuple.Create(item.TotalNumber, item.Users);
        }
    }
}
