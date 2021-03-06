using Scripts.Infrastructure.Services.Gold;
using TMPro;
using UnityEngine;
using Zenject;

namespace Logic.UI.GoldWidget
{
    public class GoldWidget : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textGold;

        private IGold _gold;
        
        [Inject]
        public void Construct(IGold gold)
        {
            _gold = gold;
            _gold.OnGoldChange += UpdateGoldText;
            
            UpdateGoldText(_gold.CurrentCount);
        }

        public void UpdateGoldText(float goldCount)
        {
            _textGold.text = Mathf.Round(goldCount).ToString();
        }

        private void OnDestroy()
        {
            _gold.OnGoldChange -= UpdateGoldText;
        }
    }
}