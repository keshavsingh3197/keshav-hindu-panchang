# Hindu Panchang & Sanskriti Hub — Keshav Singh Network

**Domain:** `https://panchang.keshavsingh.in`  
**Backend:** .NET 10 Web API (`HinduPanchang.Api`)  
**Frontend:** Angular (Standalone Components, Signals, Modern Vedic CSS)  
**Authentication:** Central Single Sign-On (SSO) via `admin.keshavsingh.in` / `id.keshavsingh.in`

---

## 🕉️ Purpose & Features

1. **Daily Vedic Panchang Engine:** Real-time computation of the 5 core astrological limbs (Tithi, Nakshatra, Yoga, Karana, Vaara), Vikram Samvat (2083), Shaka Samvat (1948), sunrise/sunset, moon timings, auspicious muhurats (Abhijit, Brahma Muhurat), inauspicious kaal (Rahu Kaal, Yamaganda, Gulika), and Day/Night Choghadiya.
2. **Aaj Ka Itihaas (Today in Indian & Vedic History):** Daily changing historical milestones, saints, freedom fighters, and daily Pauranik Katha with ethical teachings and Sanskrit mantras.
3. **Daily Curated Video Hub:** Embedded YouTube videos for daily rashifal, spiritual news, bhajans, and temple darshan. Drives viral engagement and YouTube channel subscribers.
4. **Monetization & Traffic Channelling:** Cross-promotes Keshav AnimStudio AI (`studio.keshavsingh.in`), Social Analytics (`social.keshavsingh.in`), and affiliate devotional samagri.
5. **Role-Gated Content Management:** Editors and Admins authenticated via central SSO can publish and update daily video links and announcements (`/manage`).

---

## 🏗️ Ecosystem Integration

- **SSO Identity Provider:** `https://admin.keshavsingh.in` / `https://id.keshavsingh.in`
- **AnimStudio AI:** `https://studio.keshavsingh.in`
- **Social Analytics:** `https://social.keshavsingh.in`
- **Ghar Ledger:** `https://ghar.keshavsingh.in`
- **Pratiyogita:** `https://pratiyogita.keshavsingh.in`

---

## 🚀 Local Development

### Backend (.NET 10 API)
```powershell
cd D:\GITHUB\hindu-panchang\backend\HinduPanchang.Api
dotnet run
# Runs on http://localhost:5090 (Swagger available at /swagger)
```

### Frontend (Angular)
```powershell
cd D:\GITHUB\hindu-panchang\frontend
npm install
npm start
# Runs on http://localhost:4400
```

---

## 📦 Deployment

- **Frontend:** Automated GitHub Actions build deployed to GitHub Pages via `.github/workflows/deploy-frontend.yml` pointing to `panchang.keshavsingh.in`.
- **Backend:** Dockerized service deployable to Render via `render.yaml`.
