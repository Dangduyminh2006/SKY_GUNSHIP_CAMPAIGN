# HƯỚNG DẪN KIỂM THỬ, VẬN HÀNH & MỞ RỘNG ĐỒ ÁN: SKY GUNSHIP CAMPAIGN

Tài liệu này hướng dẫn cách mở dự án trên **Unity 6 (`6000.6.7f1`)**, kiểm tra **Màn hình Toàn cảnh Trước khi Bắt đầu (Pre-Start Scene Panorama)**, điều khiển trong game và tùy chỉnh nhanh các thông số khi bảo vệ đồ án.

---

## 1. Hướng dẫn Mở Dự án & Quan sát Màn hình Toàn cảnh

1. Mở dự án `d:\Do an 2026\MTA\Unity\Project` bằng **Unity 6 (`6000.6.7f1`)**.
2. Trong cửa sổ **Project**, mở Scene chính tại đường dẫn:
   - [`Assets/Scenes/FlappyBeatUp_MainScene.unity`](file:///d:/Do%20an%202026/MTA/Unity/Project/Assets/Scenes/FlappyBeatUp_MainScene.unity)
3. *(Tùy chọn)* Nếu muốn tái tạo lại toàn bộ Scene & Prefabs về trạng thái chuẩn bất cứ lúc nào, trên thanh menu trên cùng của Unity Editor, chọn:
   - **`Sky Gunship Campaign` $\rightarrow$ `Rebuild Panoramic Scene (Stage 1-4)`**
4. **Quan sát trước khi bắt đầu (trên cả tab `#Scene` và tab `#Game` khi vừa bấm `Play`):**
   - **Bên trái (`X = -5.2`):** Máy bay chiến đấu màu đỏ (`TwinTowerFly`) đang quay cánh quạt và lơ lửng nhẹ nhàng.
   - **Ở giữa (`X = 1.0, 5.5, 10.0`):** 3 cổng chướng ngại vật núi đá (`Obstacle_Stage_1..3`) cùng các ngôi sao vàng (`starGold`) nằm giữa khe bay.
   - **Bên phải (`X = 14.8`):** Chiến hạm Boss màu xanh khổng lồ (`Boss`) đang lơ lửng chờ giao chiến.
   - **Trung tâm:** Biểu ngữ **`GET READY!`** + biểu tượng **`TAP`** và bảng hướng dẫn trực quan các Stage của trò chơi.

---

## 2. Bảng Điều khiển trong Trò chơi (Controls)

| Phím / Thao tác | Chức năng |
| :--- | :--- |
| **`Space`** hoặc **`Chuột trái (Left Click)`** | Bắt đầu ván chơi từ màn hình Toàn cảnh / Vỗ cánh bay lên |
| **Tự động (Auto-Fire)** hoặc giữ **`F` / `Chuột phải`** | Xả đạn Laser xanh (`laserGreen`) liên tục về phía trước |
| **`Space`** hoặc **`R`** (Khi Game Over / Victory) | Khởi động lại màn chơi về trạng thái Toàn cảnh ban đầu |

---

## 3. Mẹo Tinh chỉnh Thông số Nhanh khi Demo Bảo vệ Đồ án

Khi giảng viên yêu cầu thay đổi độ khó hoặc gọi Boss ra sớm để kiểm tra nhanh, bạn có thể chỉnh trực tiếp trên cửa sổ **Inspector**:

### 3.1. Gọi Boss xuất hiện ngay sau 1 hoặc 2 điểm
- Chọn đối tượng **`Logic Manager`** trong **Hierarchy** ([`LogicScript.cs`](file:///d:/Do%20an%202026/MTA/Unity/Project/Assets/Scripts/LogicScript.cs)):
  - Đổi **`Score To Spawn Boss`** từ `10` thành `1` hoặc `2`.

### 3.2. Tăng tốc độ bắn hoặc làm Máy bay dễ điều khiển hơn
- Chọn đối tượng **`TwinTowerFly`** trong **Hierarchy** ([`BirdScript.cs`](file:///d:/Do%20an%202026/MTA/Unity/Project/Assets/Scripts/BirdScript.cs)):
  - **`Flap Strength`**: Chỉnh lực bật nhảy (mặc định `8.5`).
  - **`Fire Rate`**: Giảm từ `0.30` xuống `0.12` để bắn đạn Laser liên thanh cực nhanh.

### 3.3. Chỉnh máu Boss và Tốc độ lao húc
- Chọn đối tượng **`Boss`** trong **Hierarchy** ([`BossScript.cs`](file:///d:/Do%20an%202026/MTA/Unity/Project/Assets/Scripts/BossScript.cs)):
  - **`Max HP`**: Giảm từ `50` xuống `15` để demo thắng Boss nhanh.
  - **`Dash Speed`**: Tăng tốc độ lao húc của Boss (mặc định `22.0`).

