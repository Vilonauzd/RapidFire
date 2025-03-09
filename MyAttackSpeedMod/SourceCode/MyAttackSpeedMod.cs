using System;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MyAttackSpeedMod
{
    public class MyAttackSpeedModSubModule : MBSubModuleBase
    {
        private static float _attackSpeedMultiplier = 1.0f;
        private static bool _burstFireEnabled = false;

        protected override void OnGameStart(Game game, IGameStarter gameInitializerObject)
        {
            if (game.GameType is CampaignGameType)
            {
                var campaignGameStarter = (CampaignGameStarter)gameInitializerObject;
                campaignGameStarter.AddModel(new AttackSpeedAdjustmentModel());
            }
        }

        protected override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new AttackSpeedAndBurstFireBehavior());
        }

        public override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        private class AttackSpeedAdjustmentModel : CampaignBehaviorBase
        {
            private GauntletLayer _gauntletLayer;
            private GauntletMovie _movie;

            public override void RegisterEvents()
            {
                CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            }

            public override void SyncData(IDataStore dataStore)
            {
            }

            private void OnSessionLaunched(CampaignGameStarter gameStarter)
            {
                _gauntletLayer = new GauntletLayer(1000)
                {
                    IsFocusLayer = true
                };

                _movie = _gauntletLayer.LoadMovie("AttackSpeedSlider", null);
                _movie.EventFired += OnEventFired;

                var sliderDataSource = new SliderDataSource();
                sliderDataSource.ValueChanged += (value) =>
                {
                    _attackSpeedMultiplier = value;
                    InformationManager.DisplayMessage(new InformationMessage($"Attack Speed Set to {_attackSpeedMultiplier}"));
                };

                var burstFireToggleDataSource = new ToggleDataSource();
                burstFireToggleDataSource.IsToggledChanged += (isToggled) =>
                {
                    _burstFireEnabled = isToggled;
                    InformationManager.DisplayMessage(new InformationMessage($"Burst Fire Mode: {(isToggled ? "Enabled" : "Disabled")}", isToggled ? Color.White : Color.Red));
                };

                _movie.SetVariable("AttackSpeedSlider", sliderDataSource);
                _movie.SetVariable("BurstFireToggle", burstFireToggleDataSource);

                gameStarter.AddLayer(_gauntletLayer);
            }

            private void OnEventFired(string id, object obj)
            {
            }
        }

        private class SliderDataSource : ViewModel
        {
            public event Action<float> ValueChanged;

            private float _value;
            [DataSourceProperty]
            public float Value
            {
                get => _value;
                set
                {
                    if (SetField(ref _value, value))
                    {
                        ValueChanged?.Invoke(_value);
                    }
                }
            }

            public SliderDataSource()
            {
                _value = 1.0f; // Default attack speed multiplier
            }
        }

        private class ToggleDataSource : ViewModel
        {
            public event Action<bool> IsToggledChanged;

            private bool _isToggled;
            [DataSourceProperty]
            public bool IsToggled
            {
                get => _isToggled;
                set
                {
                    if (SetField(ref _isToggled, value))
                    {
                        IsToggledChanged?.Invoke(_isToggled);
                    }
                }
            }

            public ToggleDataSource()
            {
                _isToggled = false; // Default burst fire disabled
            }
        }

        public static float GetAttackSpeedMultiplier()
        {
            return _attackSpeedMultiplier;
        }

        public static bool IsBurstFireEnabled()
        {
            return _burstFireEnabled;
        }
    }
}
