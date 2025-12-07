using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    public enum EvidenceType
    {
        Photo,
        Document,
        Object,
        Digital,
        Witness
    }

    // ✅ เพิ่ม enum สำหรับประเภทการหักล้าง
    public enum ContradictionType
    {
        StatementVsStatement,   // คำพูด vs คำพูด
        EvidenceVsStatement,    // หลักฐาน vs คำพูด
        EvidenceVsEvidence      // หลักฐาน vs หลักฐาน
    }

    [System.Serializable]
    public class StatementData
    {
        public string id;
        public string speakerName;
        [TextArea] public string text;
        public Sprite portrait;
        public bool isVerified;
        public bool isContradicted;
        public bool isUnlocked;
    }

    [System.Serializable]
    public class EvidenceData
    {
        public string id;
        public string itemName;
        [TextArea(2, 4)]
        public string description;
        public EvidenceType type;
        public Sprite icon;
        public string locationFound;
        public bool isUsed;
        public bool isUnlocked;

        public string GetFullDescription()
        {
            string typeIcon = GetTypeIcon();
            return $"{typeIcon} <b>{itemName}</b>\n\n{description}\n\n<i>เจอที่: {locationFound}</i>";
        }

        string GetTypeIcon()
        {
            switch (type)
            {
                case EvidenceType.Photo: return "📸";
                case EvidenceType.Document: return "📄";
                case EvidenceType.Object: return "🔍";
                case EvidenceType.Digital: return "💾";
                case EvidenceType.Witness: return "👁️";
                default: return "❓";
            }
        }
    }

    // ✅ เพิ่ม ContradictionRule สำหรับระบบหักล้าง
    [System.Serializable]
    public class ContradictionRule
    {
        [Header("Contradiction Pair")]
        [Tooltip("ID ของ Statement หรือ Evidence อันแรก")]
        public string itemA_ID;

        [Tooltip("ID ของ Statement หรือ Evidence อันที่สอง")]
        public string itemB_ID;

        [Header("Type")]
        [Tooltip("ประเภทของการหักล้าง")]
        public ContradictionType type;

        [Header("Result Message")]
        [Tooltip("หัวข้อที่จะแสดงเมื่อหักล้างสำเร็จ")]
        public string resultTitle = "❌ ขัดแย้ง!";

        [TextArea(3, 5)]
        [Tooltip("ข้อความที่จะแสดงเมื่อหักล้างสำเร็จ")]
        public string resultMessage = "พบความขัดแย้งระหว่างสองรายการนี้!";

        [Header("Unlock on Success")]
        [Tooltip("Statement IDs ที่จะ unlock เมื่อหักล้างสำเร็จ")]
        public string[] unlockedStatements = new string[0];

        [Tooltip("Evidence IDs ที่จะ unlock เมื่อหักล้างสำเร็จ")]
        public string[] unlockedEvidences = new string[0];
    }

    [CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData")]
    public class GameData : ScriptableObject
    {
        [Header("Statements & Evidence")]
        public List<StatementData> statements = new List<StatementData>();
        public List<EvidenceData> evidences = new List<EvidenceData>();

        [Header("Contradiction Rules")]
        [Tooltip("กฎการหักล้างสำหรับระบบ Detective Board")]
        public List<ContradictionRule> contradictions = new List<ContradictionRule>();

        public StatementData GetStatement(string id)
        {
            return statements.Find(s => s.id == id);
        }

        public EvidenceData GetEvidence(string id)
        {
            return evidences.Find(e => e.id == id);
        }

        public List<StatementData> GetUnlockedStatements()
        {
            return statements.FindAll(s => s.isUnlocked);
        }

        public List<EvidenceData> GetUnlockedEvidences()
        {
            return evidences.FindAll(e => e.isUnlocked);
        }

        public ContradictionRule GetContradictionRule(string itemA, string itemB)
        {
            return contradictions.Find(c =>
                (c.itemA_ID == itemA && c.itemB_ID == itemB) ||
                (c.itemA_ID == itemB && c.itemB_ID == itemA)
            );
        }
    }
}