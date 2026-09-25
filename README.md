# 🎵 Sonara

<p align="center">
  <b>Modern full-stack music and podcast streaming platform inspired by Spotify.</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Status-In%20Development-blue?style=for-the-badge" alt="Status">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=.net&logoColor=white" alt=".NET">
  <img src="https://img.shields.io/badge/React%20Native-61DAFB?style=for-the-badge&logo=react&logoColor=black" alt="React Native">
  <img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white" alt="Redis">
</p>

---

## 🌟 About The Project

**Sonara** is a feature-rich, scalable audio streaming platform designed to provide a seamless experience for listening to music and podcasts. Built following **Clean Architecture** principles and **CQRS**, it delivers high performance.

---

## 📋 Core Modules & Features

1. **Authorization & Authentication:** Secure login/registration, JWT-based authentication, and spam-registration protection.
2. **User Profile:** Data editing, password changing, avatar upload, subscription management.
3. **Music Catalog:** Search, browsing, and filtering of tracks, albums, artists, and podcasts by genres or popularity.
4. **Playlist Management:** Creation, editing, playlist privacy settings, and adding tracks to "Favorites".
5. **Audio Player & Streaming:** Low-latency audio playback, queue management, shuffle, and repeat modes.
6. **Recommendation System:** Personalized track recommendations based on likes and listening history.
7. **Admin Module:** Moderator panel for managing users, tracks, artist rights.

---

## 🛠️ Tech Stack

### Front-end
* React Native
* TypeScript

### Back-end
* .NET 10 (ASP.NET Core)
* Clean Architecture & CQRS
* JWT Authentication
* Redis
* SignalR (Real-time features)
* Hangfire (Background jobs)
* Azure Blob Storage + CDN (Media delivery)

### Database
* PostgreSQL

---

## 🚀 Getting Started

To get a local copy up and running, follow these standard steps.

### Prerequisites
* .NET 10 SDK
* Node.js & npm / yarn
* PostgreSQL & Redis instances

### Backend Setup
1. Clone the repository:
   ```bash
   git clone [https://github.com/your-username/sonara.git](https://github.com/your-username/sonara.git)
