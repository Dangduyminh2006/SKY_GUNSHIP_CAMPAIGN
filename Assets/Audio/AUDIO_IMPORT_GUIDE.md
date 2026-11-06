# 🎵 HƯỚNG DẪN IMPORT ÂM THANH (SFX & BGM)
> Tài liệu hướng dẫn đưa âm thanh vào `Assets/Audio/`

### 1. Nguồn tạo âm thanh 8-bit miễn phí:
* **Hiệu ứng âm thanh (SFX):** Truy cập [sfxr.me](https://sfxr.me/)
  * Bấm nút **Jump** -> Export WAV -> lưu thành `flap.wav` kéo vào `Assets/Audio/SFX/`
  * Bấm nút **Laser/Shoot** -> Export WAV -> lưu thành `shoot.wav` kéo vào `Assets/Audio/SFX/`
  * Bấm nút **Pickup/Coin** -> Export WAV -> lưu thành `score.wav` kéo vào `Assets/Audio/SFX/`
  * Bấm nút **Hit/Hurt** -> Export WAV -> lưu thành `hit.wav` kéo vào `Assets/Audio/SFX/`
  * Bấm nút **Explosion** -> Export WAV -> lưu thành `explosion.wav` kéo vào `Assets/Audio/SFX/`
* **Còi báo động:** [Kenney — Digital Audio](https://kenney.nl/assets/digital-audio) (`warning.wav`).
* **Nhạc nền (BGM):** [itch.io — Free 8-Bit Music](https://itch.io/game-assets/free/tag-music/tag-8-bit) kéo vào `Assets/Audio/BGM/`.

### 2. Thiết lập Audio Import Settings:
* File SFX: `Load Type = Decompress On Load`, `Compression Format = PCM`.
* File BGM: `Load Type = Streaming`, `Compression Format = Vorbis`.

