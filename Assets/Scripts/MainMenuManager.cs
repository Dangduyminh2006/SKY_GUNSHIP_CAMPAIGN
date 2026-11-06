using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý giao diện Menu chính (MainMenuScene):
/// Cung cấp tùy chọn Bắt đầu chơi, Lịch sử đấu, Hướng dẫn điều khiển và Thoát game.
/// Đảm bảo văn bản thuần Tiếng Việt chuẩn UTF-8 có dấu đầy đủ, tuyệt đối không sử dụng emoji.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string gameSceneName = "FlappyBeatUp_MainScene";

    [Header("Visual Hover")]
    public Transform planeModel;
    public float hoverSpeed = 3.5f;
    public float hoverAmplitude = 0.28f;
    private Vector3 initialPlanePos;

    private bool showInstructions = false;
    private bool showHistory = false;

    void Start()
    {
        if (planeModel != null)
        {
            initialPlanePos = planeModel.position;
        }

        // Tự động nạp sprite cho nền và máy bay ở Menu nếu thiếu
        LogicScript.HydrateSceneSprites();
    }

    void Update()
    {
        // Hiệu ứng máy bay lơ lửng điều hòa tại màn hình Menu
        if (planeModel != null)
        {
            float newY = initialPlanePos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            planeModel.position = new Vector3(initialPlanePos.x, newY, initialPlanePos.z);
            planeModel.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * hoverSpeed) * 3.5f);
        }

        // Phím tắt điều khiển nhanh
        if (Input.GetKeyDown(KeyCode.H))
        {
            showHistory = !showHistory;
            if (showHistory) showInstructions = false;
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            showInstructions = !showInstructions;
            if (showInstructions) showHistory = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (showInstructions)
            {
                showInstructions = false;
            }
            else if (showHistory)
            {
                showHistory = false;
            }
            else
            {
                StartGame();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showInstructions)
            {
                showInstructions = false;
            }
            else if (showHistory)
            {
                showHistory = false;
            }
            else
            {
                QuitGame();
            }
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ToggleInstructions()
    {
        showInstructions = !showInstructions;
        if (showInstructions) showHistory = false;
    }

    public void ToggleHistory()
    {
        showHistory = !showHistory;
        if (showHistory) showInstructions = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnGUI()
    {
        Color goldColor = new Color(1.0f, 0.88f, 0.15f);
        Color warmAmber = new Color(1.0f, 0.68f, 0.18f);
        Color creamWhite = new Color(1.0f, 0.97f, 0.88f);
        Color emeraldGreen = new Color(0.25f, 0.98f, 0.45f);

        // 1. Khung Tiêu Đề Game ở phần trên màn hình (Nền tối tương phản cao)
        float titleW = Mathf.Min(Screen.width - 40f, 720f);
        float titleX = (Screen.width - titleW) * 0.5f;
        float titleY = 24f;

        LogicScript.DrawContrastBox(new Rect(titleX, titleY, titleW, 86f), 0.92f);

        GUIStyle mainTitleStyle = new GUIStyle(GUI.skin.label);
        mainTitleStyle.fontSize = 32;
        mainTitleStyle.fontStyle = FontStyle.Bold;
        mainTitleStyle.alignment = TextAnchor.MiddleCenter;
        LogicScript.DrawShadowLabel(new Rect(titleX, titleY + 6, titleW, 42), "SKY GUNSHIP CAMPAIGN", mainTitleStyle, goldColor);

        GUIStyle subTitleStyle = new GUIStyle(GUI.skin.label);
        subTitleStyle.fontSize = 15;
        subTitleStyle.fontStyle = FontStyle.Bold;
        subTitleStyle.alignment = TextAnchor.MiddleCenter;
        LogicScript.DrawShadowLabel(new Rect(titleX, titleY + 50, titleW, 26), "CHIẾN DỊCH KHÔNG CHIẾN VƯỢT VÁCH ĐÁ VÀ ĐẠI CHIẾN TRÙM", subTitleStyle, creamWhite);

        // Huy hiệu Kỷ lục nhanh ở dưới tiêu đề
        int highScore = MatchHistoryManager.GetHighScore();
        int maxWave = MatchHistoryManager.GetMaxWave();
        float badgeW = 490f, badgeH = 28f;
        float badgeX = (Screen.width - badgeW) * 0.5f;
        float badgeY = titleY + 96f;
        LogicScript.DrawContrastBox(new Rect(badgeX, badgeY, badgeW, badgeH), 0.90f);

        GUIStyle badgeStyle = new GUIStyle(GUI.skin.label);
        badgeStyle.fontSize = 13;
        badgeStyle.fontStyle = FontStyle.Bold;
        badgeStyle.alignment = TextAnchor.MiddleCenter;
        LogicScript.DrawShadowLabel(new Rect(badgeX, badgeY, badgeW, badgeH), $"KỶ LỤC: {highScore} ĐIỂM  |  ĐỢT CAO NHẤT: ĐỢT {maxWave}", badgeStyle, warmAmber);

        // 2. Bảng Hướng Dẫn Chi Tiết (nếu showInstructions == true)
        if (showInstructions)
        {
            float modalW = Mathf.Min(Screen.width - 60f, 640f);
            float modalH = 390f;
            float modalX = (Screen.width - modalW) * 0.5f;
            float modalY = (Screen.height - modalH) * 0.5f;

            LogicScript.DrawContrastBox(new Rect(modalX, modalY, modalW, modalH), 0.94f);

            GUIStyle modalHeaderStyle = new GUIStyle(GUI.skin.label);
            modalHeaderStyle.fontSize = 20;
            modalHeaderStyle.fontStyle = FontStyle.Bold;
            modalHeaderStyle.alignment = TextAnchor.MiddleCenter;
            LogicScript.DrawShadowLabel(new Rect(modalX, modalY + 14, modalW, 30), "HƯỚNG DẪN ĐIỀU KHIỂN VÀ LUẬT CHƠI", modalHeaderStyle, goldColor);

            GUIStyle contentStyle = new GUIStyle(GUI.skin.label);
            contentStyle.fontSize = 14;
            contentStyle.fontStyle = FontStyle.Bold;
            contentStyle.wordWrap = true;
            contentStyle.alignment = TextAnchor.UpperLeft;

            string guideText = 
                "- Điều khiển bay: Nhấn [SPACE] hoặc [CHUỘT TRÁI] để đập cánh nâng độ cao.\n\n" +
                "- Tự động khai hỏa: Phi cơ tự động bắn tia Laser vàng phá hủy mục tiêu phía trước.\n\n" +
                "- Tích lũy điểm số: Lượn khéo qua khe hẹp vách đá để thu thập các Ngôi sao vàng.\n\n" +
                "- Đại chiến Trùm: Khi đạt mốc điểm mỗi đợt, Chiến Hạm Trùm sẽ xuất hiện. Né tránh đòn húc và đạn chùm để bắn hạ Trùm!\n\n" +
                "- Nâng cấp sau Trùm: Hạ Trùm nhận 1 lượt nâng cấp Máu, Sát thương hoặc Tốc bắn. Máu Trùm đợt sau tăng thêm +100 HP (tối đa 1000 HP).\n\n" +
                "- Vật phẩm Tiếp tế: Trong hành lang bay xuất hiện Hộp Vũ Khí [Cam/Vàng/Xanh] và Hộp Tiếp Tế Máu trước trận đánh Trùm.";

            LogicScript.DrawShadowLabel(new Rect(modalX + 30, modalY + 52, modalW - 60, 265), guideText, contentStyle, creamWhite);

            GUIStyle closeBtnStyle = new GUIStyle(GUI.skin.button);
            closeBtnStyle.fontSize = 15;
            closeBtnStyle.fontStyle = FontStyle.Bold;
            closeBtnStyle.normal.textColor = goldColor;

            if (GUI.Button(new Rect(modalX + (modalW - 190) * 0.5f, modalY + modalH - 52, 190, 40), "ĐÃ HIỂU (Space / Esc)", closeBtnStyle))
            {
                showInstructions = false;
            }
            return;
        }

        // 3. Bảng Lịch Sử Đấu (nếu showHistory == true)
        if (showHistory)
        {
            float histW = Mathf.Min(Screen.width - 60f, 680f);
            float histH = 410f;
            float histX = (Screen.width - histW) * 0.5f;
            float histY = (Screen.height - histH) * 0.5f;

            LogicScript.DrawContrastBox(new Rect(histX, histY, histW, histH), 0.94f);

            GUIStyle histHeaderStyle = new GUIStyle(GUI.skin.label);
            histHeaderStyle.fontSize = 20;
            histHeaderStyle.fontStyle = FontStyle.Bold;
            histHeaderStyle.alignment = TextAnchor.MiddleCenter;
            LogicScript.DrawShadowLabel(new Rect(histX, histY + 12, histW, 28), "LỊCH SỬ CÁC VÁN ĐẤU VÀ KỶ LỤC", histHeaderStyle, goldColor);

            MatchHistoryData historyData = MatchHistoryManager.LoadHistory();

            GUIStyle subHeaderStyle = new GUIStyle(GUI.skin.label);
            subHeaderStyle.fontSize = 13;
            subHeaderStyle.fontStyle = FontStyle.Bold;
            subHeaderStyle.alignment = TextAnchor.MiddleCenter;
            LogicScript.DrawShadowLabel(new Rect(histX, histY + 42, histW, 22), 
                $"KỶ LỤC ĐIỂM: {historyData.highScore}  |  ĐỢT CAO NHẤT: ĐỢT {historyData.maxWave}  |  TỔNG TRẬN LƯU: {historyData.matches.Count}", 
                subHeaderStyle, warmAmber);

            // Tiêu đề cột
            float tableY = histY + 70f;
            GUIStyle colHeadStyle = new GUIStyle(GUI.skin.label);
            colHeadStyle.fontSize = 12;
            colHeadStyle.fontStyle = FontStyle.Bold;

            LogicScript.DrawShadowLabel(new Rect(histX + 25, tableY, 40, 20), "STT", colHeadStyle, goldColor);
            LogicScript.DrawShadowLabel(new Rect(histX + 70, tableY, 150, 20), "THỜI GIAN", colHeadStyle, goldColor);
            LogicScript.DrawShadowLabel(new Rect(histX + 230, tableY, 80, 20), "ĐỢT", colHeadStyle, goldColor);
            LogicScript.DrawShadowLabel(new Rect(histX + 320, tableY, 80, 20), "ĐIỂM", colHeadStyle, goldColor);
            LogicScript.DrawShadowLabel(new Rect(histX + 410, tableY, 110, 20), "TRÙM ĐÃ HẠ", colHeadStyle, goldColor);
            LogicScript.DrawShadowLabel(new Rect(histX + 530, tableY, 110, 20), "KẾT QUẢ", colHeadStyle, goldColor);

            // Danh sách các trận đấu gần đây (tối đa 7 trận hiển thị trên bảng)
            float rowY = tableY + 24f;
            GUIStyle rowStyle = new GUIStyle(GUI.skin.label);
            rowStyle.fontSize = 12;
            rowStyle.fontStyle = FontStyle.Bold;

            if (historyData.matches.Count == 0)
            {
                GUIStyle emptyStyle = new GUIStyle(GUI.skin.label);
                emptyStyle.fontSize = 14;
                emptyStyle.fontStyle = FontStyle.Bold;
                emptyStyle.alignment = TextAnchor.MiddleCenter;
                LogicScript.DrawShadowLabel(new Rect(histX, histY + 150, histW, 30), "Chưa có dữ liệu ván đấu nào. Hãy vào chơi để lập kỷ lục!", emptyStyle, creamWhite);
            }
            else
            {
                int count = Mathf.Min(historyData.matches.Count, 7);
                for (int i = 0; i < count; i++)
                {
                    MatchEntry m = historyData.matches[i];
                    LogicScript.DrawShadowLabel(new Rect(histX + 25, rowY, 40, 20), $"#{i + 1}", rowStyle, creamWhite);
                    LogicScript.DrawShadowLabel(new Rect(histX + 70, rowY, 150, 20), m.date, rowStyle, creamWhite);
                    LogicScript.DrawShadowLabel(new Rect(histX + 230, rowY, 80, 20), $"Đợt {m.wave}", rowStyle, creamWhite);
                    LogicScript.DrawShadowLabel(new Rect(histX + 320, rowY, 80, 20), $"{m.score}", rowStyle, goldColor);
                    LogicScript.DrawShadowLabel(new Rect(histX + 410, rowY, 110, 20), $"{m.bossKills} Trùm", rowStyle, creamWhite);

                    Color resColor = m.isVictory ? emeraldGreen : new Color(1f, 0.45f, 0.45f);
                    LogicScript.DrawShadowLabel(new Rect(histX + 530, rowY, 110, 20), m.isVictory ? "Thắng Đợt" : "Bị Hạ", rowStyle, resColor);

                    rowY += 24f;
                }
            }

            // Nút bấm ở dưới bảng lịch sử
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 14;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.normal.textColor = goldColor;

            if (GUI.Button(new Rect(histX + 40, histY + histH - 52, 170, 38), "ĐÓNG (Space/Esc)", btnStyle))
            {
                showHistory = false;
            }

            GUIStyle clearBtnStyle = new GUIStyle(GUI.skin.button);
            clearBtnStyle.fontSize = 13;
            clearBtnStyle.fontStyle = FontStyle.Bold;
            clearBtnStyle.normal.textColor = new Color(1f, 0.4f, 0.4f);

            if (GUI.Button(new Rect(histX + histW - 200, histY + histH - 52, 160, 38), "XÓA LỊCH SỬ", clearBtnStyle))
            {
                MatchHistoryManager.ClearHistory();
            }

            return;
        }

        // 4. Khung Nút Bấm Chính ở phía dưới
        float menuW = 350f;
        float menuH = 224f;
        float menuX = (Screen.width - menuW) * 0.5f;
        float menuY = Screen.height - menuH - 30f;

        LogicScript.DrawContrastBox(new Rect(menuX, menuY, menuW, menuH), 0.92f);

        GUIStyle playBtnStyle = new GUIStyle(GUI.skin.button);
        playBtnStyle.fontSize = 17;
        playBtnStyle.fontStyle = FontStyle.Bold;
        playBtnStyle.normal.textColor = goldColor;

        // Nút 1: BẮT ĐẦU CHƠI
        if (GUI.Button(new Rect(menuX + 25, menuY + 16, menuW - 50, 44), "BẮT ĐẦU CHƠI (Space/Enter)", playBtnStyle))
        {
            StartGame();
        }

        GUIStyle secBtnStyle = new GUIStyle(GUI.skin.button);
        secBtnStyle.fontSize = 14;
        secBtnStyle.fontStyle = FontStyle.Bold;
        secBtnStyle.normal.textColor = creamWhite;

        // Nút 2: LỊCH SỬ ĐẤU
        if (GUI.Button(new Rect(menuX + 25, menuY + 68, menuW - 50, 38), "LỊCH SỬ ĐẤU (Phím H)", secBtnStyle))
        {
            ToggleHistory();
        }

        // Nút 3: HƯỚNG DẪN
        if (GUI.Button(new Rect(menuX + 25, menuY + 114, menuW - 50, 38), "HƯỚNG DẪN CHƠI (Phím I)", secBtnStyle))
        {
            ToggleInstructions();
        }

        // Nút 4: THOÁT GAME
        GUIStyle quitBtnStyle = new GUIStyle(GUI.skin.button);
        quitBtnStyle.fontSize = 14;
        quitBtnStyle.fontStyle = FontStyle.Bold;
        quitBtnStyle.normal.textColor = new Color(1f, 0.45f, 0.45f);

        if (GUI.Button(new Rect(menuX + 25, menuY + 160, menuW - 50, 38), "THOÁT GAME (Phím Esc)", quitBtnStyle))
        {
            QuitGame();
        }
    }
}
