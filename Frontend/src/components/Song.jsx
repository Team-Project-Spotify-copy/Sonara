import React, { useEffect, useMemo, useRef, useState } from "react";
import { useParams } from "react-router-dom";
import { usePlayer } from "../contexts/player.store";
import { getArtist, getTrack, getTracks } from "../api/music";
import Glyph from "./player/Glyph.jsx";
import ambientGradient from "../assets/player/song-ambient.svg";
import grainOverlay from "../assets/player/rectangle-50-tile.png";
import "../css/Song.css";

function formatPlays(count) {
  if (!Number.isFinite(count)) return null;
  return `${count.toLocaleString("en-US")} plays`;
}

function SuggestionCard({ title, track, onPlay }) {
  return (
    <section className="song-card">
      <div className="song-suggestion">
        <h3 className="song-card-title">{title}</h3>
        <button
          type="button"
          className="song-suggestion-item"
          onClick={track ? onPlay : undefined}
          disabled={!track}
        >
          <span
            className="song-suggestion-cover"
            style={track?.artworkUrl ? { backgroundImage: `url(${track.artworkUrl})` } : undefined}
          />
          <span className="song-suggestion-meta">
            <span className="song-suggestion-name">{track?.title ?? "—"}</span>
            <span className="song-suggestion-artist">{track?.artistName ?? "Nothing queued"}</span>
          </span>
        </button>
      </div>
    </section>
  );
}

export default function Song() {
  const { id } = useParams();

  const {
    currentTrack,
    nextTrack,
    hasStarted,
    error,
    setQueueAndPlay,
    togglePlay,
    next,
  } = usePlayer();

  const activeTrackRef = useRef(null);
  activeTrackRef.current = hasStarted ? currentTrack : null;

  const [details, setDetails] = useState(null);
  const [artist, setArtist] = useState(null);
  const [similar, setSimilar] = useState(null);
  const [status, setStatus] = useState("loading");

  useEffect(() => {
    let cancelled = false;

    async function load() {
      const active = activeTrackRef.current;
      if (active && (!id || active.id === id)) {
        setStatus("ready");
        return;
      }

      setStatus("loading");

      try {
        const page = await getTracks({ page: 1, pageSize: 50 });
        const tracks = page.items ?? [];

        if (cancelled) return;

        if (tracks.length === 0) {
          setStatus("empty");
          return;
        }

        const startIndex = Math.max(
          0,
          id ? tracks.findIndex((track) => track.id === id) : 0,
        );

        setQueueAndPlay(tracks, startIndex);
        setStatus("ready");
      } catch {
        if (!cancelled) setStatus("error");
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  useEffect(() => {
    const trackId = currentTrack?.id;
    if (!trackId) return undefined;

    let cancelled = false;

    getTrack(trackId)
      .then((data) => {
        if (!cancelled) setDetails(data);
      })
      .catch(() => {
        if (!cancelled) setDetails(null);
      });

    return () => {
      cancelled = true;
    };
  }, [currentTrack?.id]);

  useEffect(() => {
    const artistId = currentTrack?.artistId;
    if (!artistId) return undefined;

    let cancelled = false;

    getArtist(artistId)
      .then((data) => {
        if (!cancelled) setArtist(data);
      })
      .catch(() => {
        if (!cancelled) setArtist(null);
      });

    (async () => {
      try {
        const byArtist = await getTracks({ artistId, sort: "Popular", pageSize: 5 });
        let candidate = (byArtist.items ?? []).find((t) => t.id !== currentTrack?.id);

        if (!candidate) {
          const popular = await getTracks({ sort: "Popular", pageSize: 10 });
          candidate = (popular.items ?? []).find((t) => t.id !== currentTrack?.id);
        }

        if (!cancelled) setSimilar(candidate ?? null);
      } catch {
        if (!cancelled) setSimilar(null);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [currentTrack?.artistId, currentTrack?.id]);

  const playerActive = hasStarted && Boolean(currentTrack);

  const artwork = currentTrack?.artworkUrl ?? details?.artworkUrl ?? null;
  const title = currentTrack?.title ?? "";
  const artistName = currentTrack?.artistName ?? "";

  const totalPlays = useMemo(() => {
    if (!artist?.topTracks?.length) return null;
    return artist.topTracks.reduce((sum, track) => sum + (track.playsCount ?? 0), 0);
  }, [artist]);

  const hero = (
    <>
      <div className="song-hero">
        <img className="song-hero-ambient" src={ambientGradient} alt="" />
        <div className="song-hero-blur" />
        <div
          className="song-hero-grain"
          style={{ backgroundImage: `url(${grainOverlay})` }}
        />
      </div>
      <div className="song-rightrail" />
    </>
  );

  if (status === "loading" || status === "empty" || status === "error") {
    return (
      <div className="song-page">
        {hero}
        <div className="song-cover song-cover--idle" />
        <p className="song-page-status">
          {status === "loading"
            ? "Loading…"
            : status === "empty"
              ? "No tracks in the catalog yet."
              : "Could not load the catalog."}
        </p>
      </div>
    );
  }

  return (
    <div className="song-page">
      {hero}

      {playerActive ? (
        <div
          className="song-cover song-cover--active"
          style={artwork ? { backgroundImage: `url(${artwork})` } : undefined}
          role="img"
          aria-label={title ? `${title} — ${artistName}` : "Cover art"}
        />
      ) : (
        <button
          type="button"
          className="song-cover song-cover--idle"
          style={artwork ? { backgroundImage: `url(${artwork})` } : undefined}
          onClick={togglePlay}
          aria-label={title ? `Play ${title} by ${artistName}` : "Play"}
        >
          <span className="song-cover-hint">
            <Glyph name="play" />
          </span>
        </button>
      )}

      {!playerActive && (
        <div className="song-idle-meta">
          <p className="song-idle-title">{title || "Name"}</p>
          <p className="song-idle-artist">{artistName || "Artist"}</p>
          {error && <p className="song-idle-error">{error}</p>}
        </div>
      )}

      {playerActive && (
        <div className="song-details">
          <div className="song-details-left">
            <section className="song-card">
              <div className="song-about">
                <h2 className="song-card-title">About the artist</h2>
                <div className="song-about-body">
                  <div className="song-about-row">
                    <div className="song-about-identity">
                      <p className="song-about-name">{artist?.name ?? artistName}</p>
                      <p className="song-about-stat">
                        {formatPlays(totalPlays) ?? "Plays not available"}
                      </p>
                    </div>
                    <button
                      type="button"
                      className="song-follow"
                      disabled
                      title="Following artists is not available yet"
                    >
                      Follow
                    </button>
                  </div>
                  <p className="song-about-bio">
                    {artist?.bio ?? "No description available for this artist."}
                  </p>
                </div>
              </div>
            </section>

            <SuggestionCard title="Next song" track={nextTrack} onPlay={next} />
            <SuggestionCard
              title="Similar song"
              track={similar}
              onPlay={() => similar && setQueueAndPlay([similar], 0, { autoplay: true })}
            />
          </div>

          <div className="song-lyrics">
            <div className="song-lyrics-bg" />
            <p className="song-lyrics-body">
              <strong>{title}</strong>
              {details?.albumTitle
                ? `\nFrom the ${details.albumType?.toLowerCase() ?? "release"} “${details.albumTitle}”.`
                : ""}
              {"\n\nLyrics aren’t available for this track yet."}
            </p>
            <div className="song-lyrics-fade" />
          </div>
        </div>
      )}
    </div>
  );
}
