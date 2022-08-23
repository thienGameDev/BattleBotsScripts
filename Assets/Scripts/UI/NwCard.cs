using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Mirror;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
namespace UI {
    public class NwCard : NetworkBehaviour {
        [SerializeField] private TMP_Text cardNameText;
        [SerializeField] private TMP_Text cardTypeText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text energyText;
        
        [SyncVar] private string _jointCardType;
        [SyncVar] private string _cardName;
        [SyncVar] private string _cardType;
        [SyncVar] private float _damage;
        [SyncVar] private float _health;
        [SyncVar] private byte _energy;
        [SyncVar] private Card _data;
        
        private const byte MIN_ENERGY = 1;
        private const byte MAX_ENERGY = 3;
        private const byte MIN_DMG = 10;
        private const byte MAX_DMG = 31;
        private const byte MIN_HP = 0;
        private const byte MAX_HP = 21;
        private static char _separator;
        private string _robotComponentPrefabPath;
        private bool _isAppliedDamageOnly;
        public Card CardStruct => _data;
        public struct Card {
            public GameObject cardGameObject;
            public string robotComponentPrefabPath;
            public string jointCardType;
            public float damage;
            public float health;
            public byte energy;
            public bool isSelfDestroyed;
        }
        
        #region Server
        
        public Card SetupCard(string robotComponentPrefabPath, bool isAppliedDmgOnly, bool isSelfDestroyable) {
            _robotComponentPrefabPath = robotComponentPrefabPath;
            _isAppliedDamageOnly = isAppliedDmgOnly;
            _separator = Path.AltDirectorySeparatorChar;
            Randomize();
            DisplayCard();
            _data = new Card() {
                cardGameObject = gameObject,
                robotComponentPrefabPath = _robotComponentPrefabPath,
                jointCardType = _jointCardType,
                damage = _damage,
                health = _health,
                energy = _energy,
                isSelfDestroyed = isSelfDestroyable,
            };
            return _data;
        }
        
        private string[] SplitCamelCase(string source) 
        {
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }

        private void Randomize() {
            var componentFullname = _robotComponentPrefabPath.Split(_separator)[^1];
            _jointCardType = componentFullname.Split(".")[0];
            _cardType = String.Join(" ", SplitCamelCase(componentFullname.Split(".")[^2]));
            _cardName = String.Join(" ", SplitCamelCase(componentFullname.Split(".")[^1]));
            _energy = (byte)Random.Range(MIN_ENERGY, MAX_ENERGY);
            _damage = _isAppliedDamageOnly ? Random.Range(MIN_DMG, MAX_DMG) : 0;
            _health = _isAppliedDamageOnly ? 0 : Random.Range(MIN_HP, MAX_HP);
        }

        #endregion

        #region Shared

        private void DisplayCard() {
            cardNameText.text = _cardName;
            cardTypeText.text = _cardType;
            damageText.text = _damage.ToString(CultureInfo.CurrentCulture);
            healthText.text = _health.ToString(CultureInfo.CurrentCulture);
            energyText.text = _energy.ToString();
        }
        
        #endregion

        #region Client

        public override void OnStartClient() {
            base.OnStartClient();
            DisplayCard();
            var parentTransform = GameObject.FindGameObjectWithTag("CardPileUI").transform;
            transform.SetParent(parentTransform, false);
        }
        #endregion
    }
}