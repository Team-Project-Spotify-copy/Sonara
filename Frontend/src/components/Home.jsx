import React from "react";

// Стилі для швидкої стилізації без зовнішніх CSS-файлів
const styles = {
  container: {
    backgroundColor: "#121212", // Темна тема у стилі Spotify
    color: "#ffffff",
    fontFamily: "'Montserrat', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif",
    minHeight: "100vh",
    display: "flex",
    flexDirection: "column",
    justifyContent: "space-between",
  },
  header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "20px 40px",
    backgroundColor: "#070707",
    borderBottom: "1px solid #282828",
  },
  logoContainer: {
    display: "flex",
    alignItems: "center",
    gap: "12px",
  },
  logoImg: {
    width: "40px",
    height: "40px",
    borderRadius: "50%",
    objectFit: "cover",
  },
  logoText: {
    fontSize: "24px",
    fontWeight: "bold",
    letterSpacing: "1px",
    color: "#1DB954", // Фірмовий зелений
  },
  nav: {
    display: "flex",
    gap: "20px",
  },
  navLink: {
    color: "#b3b3b3",
    textDecoration: "none",
    fontSize: "14px",
    fontWeight: "600",
    transition: "color 0.2s",
  },
  // Головний контейнер
  main: {
    maxWidth: "1200px",
    margin: "0 auto",
    padding: "40px 20px",
    width: "100%",
    boxSizing: "border-box",
  },
  // Секція Intro
  introSection: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    gap: "40px",
    marginBottom: "50px",
    flexWrap: "wrap",
  },
  introText: {
    flex: "1",
    minWidth: "300px",
  },
  introTitle: {
    fontSize: "36px",
    fontWeight: "800",
    marginBottom: "15px",
    color: "#ffffff",
  },
  introDescription: {
    fontSize: "16px",
    lineHeight: "1.6",
    color: "#b3b3b3",
  },
  sliderContainer: {
    flex: "1",
    minWidth: "300px",
    height: "220px",
    backgroundColor: "#181818",
    borderRadius: "8px",
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    border: "1px solid #282828",
    color: "#b3b3b3",
    fontWeight: "bold",
  },
  // Крапки пагінації
  paginationDots: {
    display: "flex",
    justifyContent: "center",
    gap: "8px",
    marginBottom: "40px",
  },
  dot: {
    width: "8px",
    height: "8px",
    borderRadius: "50%",
    backgroundColor: "#535353",
    cursor: "pointer",
  },
  activeDot: {
    width: "8px",
    height: "8px",
    borderRadius: "50%",
    backgroundColor: "#1DB954",
    cursor: "pointer",
  },
  // Секції з картками (Services / Recent Work)
  sectionTitle: {
    fontSize: "22px",
    fontWeight: "700",
    marginBottom: "20px",
    borderBottom: "1px solid #282828",
    paddingBottom: "10px",
  },
  grid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
    gap: "24px",
    marginBottom: "50px",
  },
  card: {
    backgroundColor: "#181818",
    borderRadius: "8px",
    padding: "30px",
    textAlign: "center",
    border: "1px solid #282828",
    transition: "background-color 0.3s",
    cursor: "pointer",
    minHeight: "150px",
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    alignItems: "center",
  },
  cardTitle: {
    fontSize: "18px",
    fontWeight: "600",
    color: "#ffffff",
  },
  // Соціальна стрічка (Twitter / Social Feed)
  socialFeed: {
    backgroundColor: "#181818",
    border: "1px solid #282828",
    borderRadius: "8px",
    padding: "25px",
    marginBottom: "50px",
    textAlign: "center",
  },
  // Футер
  footer: {
    backgroundColor: "#000000",
    padding: "40px 20px",
    borderTop: "1px solid #282828",
  },
  footerContent: {
    maxWidth: "1200px",
    margin: "0 auto",
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    flexWrap: "wrap",
    gap: "20px",
  },
  copyright: {
    color: "#727272",
    fontSize: "13px",
  },
};

export default function Home() {
  return (
    <div style={styles.container}>
      {/* HEADER */}
      <header style={styles.header}>
        <div style={styles.logoContainer}>
          <img
            src="/src/img/home_page_sonara_№1.jpg"
            alt="Sonara Logo"
            style={styles.logoImg}
            onError={(e) => {
              // Фоллбек, якщо картинка не завантажиться
              e.target.style.display = "none";
            }}
          />
          <span style={styles.logoText}>SONARA</span>
        </div>
        <nav style={styles.nav}>
          <a href="#home" style={styles.navLink}>
            Home
          </a>
          <a href="#search" style={styles.navLink}>
            Search
          </a>
          <a href="#library" style={styles.navLink}>
            Library
          </a>
          <a href="#premium" style={styles.navLink}>
            Premium
          </a>
          <a href="#download" style={styles.navLink}>
            Download
          </a>
        </nav>
      </header>

      {/* MAIN CONTENT */}
      <main style={styles.main}>
        {/* INTRO SECTION */}
        <section style={styles.introSection}>
          <div style={styles.introText}>
            <h2 style={styles.introTitle}>Artist Spotlight: Sonara Picks</h2>
            <p style={styles.introDescription}>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do
              eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut
              enim ad minim veniam, quis nostrud exercitation ullamco laboris
              nisi ut aliquip ex ea commodo consequat.
            </p>
          </div>
          <div style={styles.sliderContainer}>
            Featured Project Slider
          </div>
        </section>

        {/* PAGINATION DOTS */}
        <div style={styles.paginationDots}>
          <span style={styles.activeDot}></span>
          <span style={styles.dot}></span>
          <span style={styles.dot}></span>
          <span style={styles.dot}></span>
          <span style={styles.dot}></span>
        </div>

        {/* SERVICES / CURATED LISTS SECTION */}
        <section>
          <div style={styles.grid}>
            <div style={styles.card}>
              <h4 style={styles.cardTitle}>Services</h4>
            </div>
            <div style={styles.card}>
              <h4 style={styles.cardTitle}>Services</h4>
            </div>
            <div style={styles.card}>
              <h4 style={styles.cardTitle}>Services</h4>
            </div>
          </div>
        </section>

        {/* RECENT RELEASES SECTION */}
        <section>
          <div style={styles.grid}>
            <div style={styles.card}>
              <h4 style={styles.cardTitle}>Recent Work</h4>
            </div>
            <div style={styles.card}>
              <h4 style={styles.cardTitle}>Recent Work</h4>
            </div>
            <div style={styles.card}>
              <h4 style={styles.cardTitle}>Recent Work</h4>
            </div>
          </div>
        </section>

        {/* SOCIAL FEED SECTION */}
        <section style={styles.socialFeed}>
          <h3 style={{ ...styles.cardTitle, marginBottom: "10px" }}>
            Sonara Social Feed
          </h3>
          <p style={{ color: "#b3b3b3", fontSize: "14px" }}>
            Stay updated with our latest Twitter updates and community news.
          </p>
        </section>
      </main>

      {/* FOOTER */}
      <footer style={styles.footer}>
        <div style={styles.footerContent}>
          <nav style={styles.nav}>
            <a href="#home" style={styles.navLink}>
              Home
            </a>
            <a href="#search" style={styles.navLink}>
              Search
            </a>
            <a href="#library" style={styles.navLink}>
              Library
            </a>
            <a href="#premium" style={styles.navLink}>
              Premium
            </a>
            <a href="#download" style={styles.navLink}>
              Download
            </a>
          </nav>
          <span style={styles.copyright}>
            &copy; {new Date().getFullYear()} Sonara. All rights reserved.
          </span>
        </div>
      </footer>
    </div>
  );
}
