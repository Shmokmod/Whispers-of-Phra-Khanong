using GameSystem;
using UnityEngine;

namespace GameSystem
{
    public class GameDataRuntime : MonoBehaviour
    {
        public static GameDataRuntime Instance;
        public GameData data;
        public GameData defaultData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadGameData();
        }

        public void LoadGameData()
        {
            if (data == null || defaultData == null) return;

            for (int i = 0; i < data.statements.Count && i < defaultData.statements.Count; i++)
            {
                string key = $"stmt_{data.statements[i].id}_unlocked";

                if (!PlayerPrefs.HasKey(key))
                {
                    data.statements[i].isUnlocked = defaultData.statements[i].isUnlocked;
                }
                else
                {
                    data.statements[i].isUnlocked = PlayerPrefs.GetInt(key) == 1;
                }

                data.statements[i].isVerified = false;
                data.statements[i].isContradicted = false;
            }

            for (int i = 0; i < data.evidences.Count && i < defaultData.evidences.Count; i++)
            {
                string key = $"evid_{data.evidences[i].id}_unlocked";

                if (!PlayerPrefs.HasKey(key))
                {
                    data.evidences[i].isUnlocked = defaultData.evidences[i].isUnlocked;
                }
                else
                {
                    data.evidences[i].isUnlocked = PlayerPrefs.GetInt(key) == 1;
                }

                data.evidences[i].isUsed = false;
            }

            Debug.Log("✅ Game Data Loaded!");
        }

        // ✅ ฟังก์ชันใหม่: Unlock และ Save Statement ในคำสั่งเดียว
        public void UnlockAndSaveStatement(string stmtID)
        {
            StatementData stmt = GetStatement(stmtID);
            if (stmt != null)
            {
                stmt.isUnlocked = true;
                SaveUnlockedStatement(stmtID);
                Debug.Log($"🔓 Unlocked & Saved Statement: {stmtID}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Cannot unlock Statement {stmtID}: Not found");
            }
        }

        // ✅ ฟังก์ชันใหม่: Unlock และ Save Evidence ในคำสั่งเดียว
        public void UnlockAndSaveEvidence(string evidID)
        {
            EvidenceData evid = GetEvidence(evidID);
            if (evid != null)
            {
                evid.isUnlocked = true;
                SaveUnlockedEvidence(evidID);
                Debug.Log($"🔓 Unlocked & Saved Evidence: {evidID}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Cannot unlock Evidence {evidID}: Not found");
            }
        }

        public void SaveUnlockedStatement(string stmtID)
        {
            string key = $"stmt_{stmtID}_unlocked";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"💾 Saved Statement: {stmtID}");
        }

        public void SaveUnlockedEvidence(string evidID)
        {
            string key = $"evid_{evidID}_unlocked";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"💾 Saved Evidence: {evidID}");
        }

        public StatementData GetStatement(string id)
        {
            if (data == null)
            {
                Debug.LogError("GameData ไม่ได้ assign ใน Inspector!");
                return null;
            }
            return data.GetStatement(id);
        }

        public EvidenceData GetEvidence(string id)
        {
            if (data == null)
            {
                Debug.LogError("GameData ไม่ได้ assign ใน Inspector!");
                return null;
            }
            return data.GetEvidence(id);
        }

        // ✅ ฟังก์ชันช่วยดีบัก: ดูข้อมูลที่ Save ไว้
        [ContextMenu("Debug - Show All Saved Data")]
        public void DebugShowSavedData()
        {
            Debug.Log("=== SAVED STATEMENTS ===");
            for (int i = 0; i < data.statements.Count; i++)
            {
                string key = $"stmt_{data.statements[i].id}_unlocked";
                bool saved = PlayerPrefs.HasKey(key);
                int value = PlayerPrefs.GetInt(key, 0);
                Debug.Log($"Statement {data.statements[i].id}: Saved={saved}, Value={value}, Current isUnlocked={data.statements[i].isUnlocked}");
            }

            Debug.Log("=== SAVED EVIDENCES ===");
            for (int i = 0; i < data.evidences.Count; i++)
            {
                string key = $"evid_{data.evidences[i].id}_unlocked";
                bool saved = PlayerPrefs.HasKey(key);
                int value = PlayerPrefs.GetInt(key, 0);
                Debug.Log($"Evidence {data.evidences[i].id}: Saved={saved}, Value={value}, Current isUnlocked={data.evidences[i].isUnlocked}");
            }
        }

        // ✅ ฟังก์ชันช่วยดีบัก: ลบข้อมูลทั้งหมด
        [ContextMenu("Debug - Clear All Saved Data")]
        public void ClearAllSavedData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("🗑️ ลบข้อมูลที่ Save ไว้ทั้งหมดแล้ว");
        }

        // ✅ ฟังก์ชันช่วยดีบัก: Unlock ทุกอย่างเพื่อทดสอบ
        [ContextMenu("Debug - Unlock All")]
        public void UnlockAll()
        {
            foreach (var stmt in data.statements)
            {
                UnlockAndSaveStatement(stmt.id);
            }
            foreach (var evid in data.evidences)
            {
                UnlockAndSaveEvidence(evid.id);
            }
            Debug.Log("🔓 Unlocked ทุกอย่างแล้ว!");
        }
    }
}