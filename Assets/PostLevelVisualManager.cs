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
            "\n\nRooms Cleared: " + ADM.GetTotalRoomsCleared() +
            "   Avg Clear Time: " + ADM.GetAvgRoomClearTime() +
            "\nFloors Cleared: " + ADM.GetTotalFloorsCleared() +
            "\n\nAttacks: " + ADM.GetTotalMeleeAttacks() +
            "   Hits: " + ADM.GetTotalMeleeHits() +
            "\nAccuracy: " + ADM.GetTotalMeleeAccuracy() +
            "   Damage Dealt: " + ADM.GetTotalDamageDealt() +
            "\nCombos Performed: " + ADM.GetTotalCombosPerformed() +
            "\n\nMagic Attacks: " + ADM.GetTotalSpellAttacks() +
            "   Magic Hits: " + ADM.GetTotalSpellHits() +
            "\nMagic Accuracy: " + ADM.GetTotalSpellAccuracy() +
            "   Spell Damage Dealt: " + ADM.GetTotalSpellDamageDealt() +
            "\n\nDodges: " + ADM.GetTotalDodges() +
            "   Hits Dodged: " + ADM.GetTotalDodgesSuccessful() +
            "\nDodge Effectiveness: " + ADM.GetTotalDodgeEffectiveness() +
            "\n\nHits Taken: " + ADM.GetTimesDamageTaken().Length +
            "   Damage Taken: " + ADM.GetTotalDamageTaken() +
            "\nAvg Time Between Damage: " + ADM.GetAvgTimeBetweenDamageTaken() +
            "\n\nConsumables Used: " + ADM.GetTotalConsumablesUsed();
    }
}
