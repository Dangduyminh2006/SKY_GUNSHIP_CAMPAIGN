# 🎮 FLAPPY BEAT UP (SkyBattle)
> **Học phần:** Tương tác đối tượng trong Môi trường ảo (MTA)  
> **Đơn vị:** Khoa Công nghệ Thông tin — Trường Đại học Kinh doanh và Công nghệ Hà Nội (HUBT)  
> **Nền tảng:** Unity 6 (`6000.x` - 2D Core) | **Ngôn ngữ:** C# | **Tỷ lệ chuẩn:** 16:9 (`1920x1080`)

---

## 📖 GIỚI THIỆU DỰ ÁN

**"Flappy Beat Up"** là một tựa game hành động 2D kết hợp đột phá giữa hai dòng game kinh điển:
1. **Lối chơi Vượt Chướng Ngại Vật Trọng Lực (Flappy Bird):** Giữ độ cao khéo léo qua các cặp ống cống hẹp để tích lũy điểm số.
2. **Lối chơi Giao Tranh Đấu Trùm (Shoot 'em up / Boss Rush - Cuphead):** Khi đạt mốc 10 điểm, hệ thống kích hoạt còi báo động khẩn cấp và đưa người chơi vào trận đấu sinh tử với Siêu Trùm Cơ Khí **Mecha Eagle**. Người chơi vừa bay né đạn chùm và các cú húc tốc độ cao, vừa xả hỏa lực laser liên thanh để hạ gục Boss.

---

## 🕹️ PHÍM ĐIỀU KHIỂN (CONTROLS)

| Phím / Thao tác | Hành Động Gameplay | Chi Tiết |
| :--- | :--- | :--- |
| **`Phím Space`** hoặc **`Chuột Trái`** | Bay lên / Vỗ cánh | Tạo lực đẩy tức thời chống lại trọng lực, máy bay ngóc đầu lên theo vận tốc. |
| **Tự động bắn (`Auto-fire`)** | Xả đạn liên thanh | Đạn laser tự động bắn thẳng từ mỏ chim với chu kỳ $0.35\text{s/viên}$. |
| **`Phím F`** hoặc **`Chuột Phải`** | Bắn thủ công | Cho phép nhấp bắn chủ động theo ý muốn. |
| **`Phím R`** hoặc **`Space`** | Chơi lại tức thì | Khởi động lại ván chơi mới ngay lập tức khi Game Over hoặc Chiến Thắng. |

---

## 🌟 ĐẶC TRƯNG KỸ THUẬT NỔI BẬT (MTA HIGHLIGHTS)

* **Vật lý 2D Chân thực & Chống Lỗi Biên (Edge Cases):**
  * Tích hợp cơ chế kiểm soát ranh giới trần và đáy màn hình, triệt tiêu lỗi gian lận bay vượt nóc.
  * Kiểm tra trạng thái sống của chim trước khi tính điểm tại khe ống, chống lỗi cộng điểm sau khi tử nạn.
* **Tối ưu hóa Bộ nhớ với Object Pooling (`ObjectPooler.cs`):**
  * Tái sử dụng toàn bộ đạn người chơi và đạn trùm thông qua cấu trúc hàng đợi `Queue<GameObject>`, ngăn chặn hiện tượng khựng khung hình (GC Spikes).
* **Trí tuệ Nhân tạo Máy Trạng thái Hữu hạn (`BossScript.cs` - FSM):**
  * Boss Mecha Eagle sở hữu 6 pha tác chiến: *Xuất hiện (Entering) $\to$ Lượn sóng sin (Idle) $\to$ Bắn chùm đạn 3 hướng $\to$ Húc Dash ép góc $\to$ Cuồng nộ (Enraged) khi máu dưới 50% $\to$ Nổ tung (Dead)*.
* **Đồ họa & Âm thanh Tinh gọn (Juice & Audio):**
  * Hiệu ứng rung chấn màn hình (`CameraShake.cs`) khi va chạm hoặc trúng đạn.
  * Hiệu ứng chớp trắng giáp Boss khi nhận sát thương.
  * 2 bản BGM thích ứng theo ngữ cảnh và 7 hiệu ứng SFX retro 8-bit sống động.
* **Hệ thống Giao diện Tự hành (Smart HUD):**
  * Tự động hiển thị điểm số, thanh máu Boss và màn hình Game Over / Victory độc lập thông qua OnGUI.

---

## 🚀 HƯỚNG DẪN KHỞI CHẠY TRONG UNITY

1. Mở **Unity Hub** $\rightarrow$ chọn **Add project from disk** $\rightarrow$ trỏ đến thư mục `Project/`.
2. Mở Scene chính: `Assets/Scenes/FlappyBeatUp_MainScene.unity`.
3. Bấm nút **Play (▶)** ở thanh công cụ trên cùng để trải nghiệm game.

---

## 📁 TÀI LIỆU DỰ ÁN ĐÍNH KÈM

* 📐 **[Tài liệu Thiết kế Hệ thống (SDD)](docs/01_SYSTEM_DESIGN_DOCUMENT.md):** Bản đặc tả kiến trúc kỹ thuật chi tiết.
* 🗺️ **[Kế hoạch Phát triển Phân kỳ (Roadmap)](docs/02_PHASED_DEVELOPMENT_PLAN.md):** Lộ trình 5 giai đoạn thực hiện.
* 📑 **[Báo cáo Đồ án 3 Chương](docs/03_BAO_CAO_DO_AN_3_CHUONG.md):** Báo cáo môn học MTA chuẩn cấu trúc HUBT.
* 🎮 **[Cẩm nang Hướng dẫn Kiểm thử Gameplay](docs/04_HUONG_DAN_KIEM_THU_GAMEPLAY.md):** Kịch bản kiểm thử chất lượng QA chi tiết.

