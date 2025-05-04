using TMPro;
using UnityEngine;

public class PostLevelVisualManager : MonoBehaviour
{
    [SerializeField] private TMP_Text visualText;
    [SerializeField] private TMP_Text visualHeader;
    private AbstractSceneManager ASM;
    private AdaptiveDifficultyManager ADM;

    public void Wake(AbstractSceneManager newASM)
    {
        ASM = newASM;
        ADM = ASM.GetADM();
    }

    public void SetVisualHeader(string newHeader)
    {
        visualHeader.text = newHeader;
    }

    public void UpdateVisualText()
    {
        visualText.text =
            "Skill Score: " + ADM.GetSkillScore() +
            "   Difficulty: " + ADM.GetDifficulty() +
            "\n\nRooms Cleared: " + ADM.GetRoomsCleared() +
            "   Avg Clear Time: " + ADM.GetAvgRoomClearTime() +
            "\n\nAttacks: " + ADM.GetNumOfAttacks() +
            "   Hits: " + ADM.GetNumOfHits() +
            "\nAccuracy: " + ADM.GetAccuracy() +
            "   Damage Dealt: " + ADM.GetTotalDamageDealt() +
            "\nCombos Performed: " + ADM.GetCombosPerformed() +
            "\n\nMagic Attacks: " + ADM.GetNumOfSpellAttacks() +
            "   Magic Hits: " + ADM.GetNumOfSpellHits() +
            "\nMagic Accuracy: " + ADM.GetSpellAccuracy() +
            "   Spell Damage Dealt: " + ADM.GetTotalSpellDamageDealt() +
            "\n\nDodges: " + ADM.GetNumOfDodges() +
            "   Hits Dodged: " + ADM.GetNumOfHitsDodged() +
            "\nDodge Effectiveness: " + ADM.GetDodgeEffectiveness() +
            "\n\nHits Taken: " + ADM.GetTimesDamageTaken().Length +
            "   Damage Taken: " + ADM.GetTotalDamageTaken() +
            "\nAvg Time Between Damage: " + ADM.GetAvgTimeBetweenDamageTaken() +
            "\n\nConsumables Used: " + ADM.GetConsumablesUsed();
    }
}
