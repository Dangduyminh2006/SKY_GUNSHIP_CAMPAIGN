# 🎨 HƯỚNG DẪN IMPORT SPRITES & ASSETS
> Tài liệu hướng dẫn đưa hình ảnh vào thư mục `Assets/Sprites/`

### 1. Nguồn tải Assets chuẩn (Theo Plan):
* **Nhân vật, Ống & Bầu trời:** [Kenney — Tappy Plane](https://kenney.nl/assets/tappy-plane) (Nhấn nút "Download" miễn phí CC0).
* **Giao diện (UI):** [Kenney — UI Pack Pixel Adventure](https://kenney.nl/assets/ui-pack-pixel-adventure) (Kéo vào thư mục `Assets/Sprites/UI/`).
* **Boss:** [itch.io — 2D Flying Boss Free](https://itch.io/game-assets/free/tag-2d/tag-boss).

### 2. Thiết lập sau khi kéo ảnh vào Unity Editor:
Chọn tất cả ảnh trong thư mục `Assets/Sprites/` và chỉnh thông số trên cửa sổ **Inspector**:
1. **Texture Type:** `Sprite (2D and UI)`
2. **Sprite Mode:** `Single` (nếu là ảnh rời) hoặc `Multiple` (nếu là Sprite Sheet)
3. **Filter Mode:** `Point (no filter)` (Bắt buộc để pixel art không bị mờ nhòe)
4. **Compression:** `None`
5. Nhấn **Apply**.

