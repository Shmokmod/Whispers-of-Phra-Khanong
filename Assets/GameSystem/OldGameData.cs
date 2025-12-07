//using System.Collections.Generic;

//public static class GameData
//{
//    public static Dictionary<string, Statement> allStatements = new();
//    public static Dictionary<string, Evidence> allEvidences = new();

//    // จำว่าเราปลดล็อกการ์ดไหนบ้าง
//    public static HashSet<string> unlockedStatements = new();
//    public static HashSet<string> unlockedEvidences = new();

//    public static Statement GetStatement(string id)
//    {
//        if (!unlockedStatements.Contains(id)) return null; // ยังไม่ได้ปลดล็อก
//        return allStatements.ContainsKey(id) ? allStatements[id] : null;
//    }

//    public static Evidence GetEvidence(string id)
//    {
//        if (!unlockedEvidences.Contains(id)) return null;
//        return allEvidences.ContainsKey(id) ? allEvidences[id] : null;
//    }
//}






















/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

















//using UnityEngine;
//using System.Collections.Generic;

//public static class GameData
//{
//    // =======================================
//    // STATEMENTS
//    // =======================================
//    public static Statement GetStatement(string id)
//    {
//        switch (id)
//        {
//            case "stmt_guard_time":
//                return new Statement
//                {
//                    id = "stmt_guard_time",
//                    speakerName = "ยาม A",
//                    text = "ผมเห็นหุ้นส่วนที่โกดังเวลา 20:00 น. แน่นอน",
//                    portrait = Resources.Load<Sprite>("Portraits/Guard_A"),
//                    locationFound = "โกดัง"
//                };

//            case "stmt_worker_saw_guard":
//                return new Statement
//                {
//                    id = "stmt_worker_saw_guard",
//                    speakerName = "คนงาน C",
//                    text = "ผมเห็นยามออกจากโกดังประมาณ 19:00 น.",
//                    portrait = Resources.Load<Sprite>("Portraits/Worker_C"),
//                    locationFound = "โกดัง"
//                };

//            case "stmt_partner_location":
//                return new Statement
//                {
//                    id = "stmt_partner_location",
//                    speakerName = "หุ้นส่วน D",
//                    text = "ผมอยู่ที่บ้านตลอดทั้งคืน ไม่ได้ไปไหน",
//                    portrait = Resources.Load<Sprite>("Portraits/Partner_D"),
//                    locationFound = "สำนักงาน"
//                };

//            default:
//                Debug.LogError($"Statement not found: {id}");
//                return null;
//        }
//    }

//    // =======================================
//    // EVIDENCE
//    // =======================================
//    public static Evidence GetEvidence(string id)
//    {
//        switch (id)
//        {
//            case "evid_cctv_footage":
//                return new Evidence
//                {
//                    id = "evid_cctv_footage",
//                    itemName = "CCTV Footage",
//                    description = "แสดงหุ้นส่วนอยู่ที่ร้านกาแฟระหว่าง 19:30-21:00 น.",
//                    type = EvidenceType.Digital,
//                    icon = Resources.Load<Sprite>("Evidence/CCTV"),
//                    locationFound = "ร้านกาแฟ"
//                };

//            case "evid_timecard":
//                return new Evidence
//                {
//                    id = "evid_timecard",
//                    itemName = "บัตรเวลาของยาม",
//                    description = "แสดงว่ายามเข้างานเวลา 18:45 น.",
//                    type = EvidenceType.Document,
//                    icon = Resources.Load<Sprite>("Evidence/Timecard"),
//                    locationFound = "ห้องยาม"
//                };

//            case "evid_security_log":
//                return new Evidence
//                {
//                    id = "evid_security_log",
//                    itemName = "บันทึกความปลอดภัย",
//                    description = "มีการบันทึกว่ายามออกจากโกดัง 19:05 น.",
//                    type = EvidenceType.Document,
//                    icon = Resources.Load<Sprite>("Evidence/Log"),
//                    locationFound = "โกดัง"
//                };

//            case "evid_neighbor_testimony":
//                return new Evidence
//                {
//                    id = "evid_neighbor_testimony",
//                    itemName = "คำให้การเพื่อนบ้าน",
//                    description = "เห็นหุ้นส่วนออกจากบ้านเวลา 19:00 น.",
//                    type = EvidenceType.Witness,
//                    icon = Resources.Load<Sprite>("Evidence/Witness"),
//                    locationFound = "บ้านหุ้นส่วน"
//                };

//            default:
//                Debug.LogError($"Evidence not found: {id}");
//                return null;
//        }
//    }
//}