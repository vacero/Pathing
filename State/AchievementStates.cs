﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public class AchievementStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<AchievementStates>();

        private const double INTERVAL_CHECKACHIEVEMENTS = 300010; // 5 minutes + 10ms

        private double _lastAchievementCheck = INTERVAL_CHECKACHIEVEMENTS;

        private readonly ConcurrentDictionary<int, AchievementStatus> _achievementStates = new();

        public AchievementStates(IRootPackState rootPackState) : base(rootPackState) { }

        public override Task Reload() {
            return Task.CompletedTask;
        }

        public override void Update(GameTime gameTime) {
            // TODO: Switch this to the async version
            UpdateCadenceUtil.UpdateWithCadence(UpdateAchievements, gameTime, INTERVAL_CHECKACHIEVEMENTS, ref _lastAchievementCheck);
        }

        protected override Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public override Task Unload() {
            return Task.CompletedTask;
        }

        public bool IsAchievementHidden(int achievementId, int achievementBit) {
            AchievementStatus achievement;

            // If the achievement is not found, we show it.
            lock (_achievementStates) if (!_achievementStates.TryGetValue(achievementId, out achievement)) return false;

            // If the achievement reports that it has been completed, we hide it.
            if (achievement.Done) return true;

            // If the achievement is partially done and this bit has been completed, we hide it.
            return achievement.AchievementBits.Contains(achievementBit);
        }

        private void HandleAchievementUpdate(Task<IApiV2ObjectList<AccountAchievement>> accountAchievementTask) {
            lock (_achievementStates) {
                foreach (var achievement in accountAchievementTask.Result) {
                    _achievementStates.AddOrUpdate(achievement.Id,
                                                   new AchievementStatus(achievement),
                                                   (_, _) => new AchievementStatus(achievement));
                }
            }
        }

        private void UpdateAchievements(GameTime gameTime) {
            if (!this.Running) return;

            try {
                // v2/account/achivements requires "account" and "progression" permissions.
                if (PathingModule.Instance.Gw2ApiManager.HavePermissions(new[] { TokenPermission.Account, TokenPermission.Progression })) {
                    Logger.Debug("Getting user achievements from the API.");
                    PathingModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync().ContinueWith(HandleAchievementUpdate, TaskContinuationOptions.NotOnFaulted);
                } else {
                    Logger.Debug("Skipping user achievements from the API - API key does not give us permission.");
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to load account achievements.");
            }
        }

    }
}
