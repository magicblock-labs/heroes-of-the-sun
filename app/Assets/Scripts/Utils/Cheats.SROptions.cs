using Connectors;
using Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Injection;
using Utils;

namespace StompyRobot.SROptions
{
    public partial class SROptions
    {
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private SettlementModel _model;

        public SROptions()
        {
            Injector.Instance.Resolve(this);
        }

        public void ClearPreferences()
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(0);
        }

        public async void Reset()
        {
            var curr = PlayerPrefs.GetInt("ACC_BUMP", 0);
            PlayerPrefs.SetInt("ACC_BUMP", ++curr);

            SceneManager.LoadScene(0);
        }

        public async void ClaimLoot()
        {
            var connector = (LootDistributionConnector)Injector.Instance.GetValue(typeof(LootDistributionConnector));
            await connector.Claim(0);
        }

        public async void CreateDiety()
        {
            var loc = (SmartObjectLocationConnector)Injector.Instance.GetValue(typeof(SmartObjectLocationConnector));
            var hero = (PlayerHeroModel)Injector.Instance.GetValue(typeof(PlayerHeroModel));
            await loc.SetSeed($"TL@{hero.Get().X}x{hero.Get().Y}");
            await loc.Init(hero.Get().X, hero.Get().Y);

            var deity =
                (SmartObjectDeityConnector)Injector.Instance.GetValue(typeof(SmartObjectDeityConnector));
            await deity.SetEntityPda(loc.EntityPda, false, true);
            await deity.Initialize();
        }
    }
}