import React, { useCallback, useState } from "react";
import { usePlayer } from "../../contexts/player.store";
import Glyph from "./Glyph.jsx";
import { formatTime } from "../../utils/time";
import "../../css/QueueDrawer.css";

/**
 * Згортна панель «Up next». Читає чергу з контексту, вміє прибирати треки
 * і міняти їхній порядок перетягуванням — усі три операції йдуть у контекст,
 * власного стану черги панель не тримає.
 */
export default function QueueDrawer() {
  const {
    queue,
    index,
    queueOpen,
    closeQueue,
    goTo,
    removeFromQueue,
    moveInQueue,
  } = usePlayer();

  // Індекс рядка, який тягнуть, і рядка, над яким зараз курсор.
  const [draggingIndex, setDraggingIndex] = useState(null);
  const [dropIndex, setDropIndex] = useState(null);

  const onDragStart = useCallback((event, position) => {
    setDraggingIndex(position);
    event.dataTransfer.effectAllowed = "move";
    // Firefox ігнорує drag без даних у dataTransfer.
    event.dataTransfer.setData("text/plain", String(position));
  }, []);

  const onDragOver = useCallback((event, position) => {
    event.preventDefault();
    event.dataTransfer.dropEffect = "move";
    setDropIndex(position);
  }, []);

  const onDrop = useCallback(
    (event, position) => {
      event.preventDefault();

      const from = Number(event.dataTransfer.getData("text/plain"));
      if (Number.isInteger(from)) moveInQueue(from, position);

      setDraggingIndex(null);
      setDropIndex(null);
    },
    [moveInQueue],
  );

  const onDragEnd = useCallback(() => {
    setDraggingIndex(null);
    setDropIndex(null);
  }, []);

  if (!queueOpen) return null;

  return (
    <aside className="queue" aria-label="Up next">
      <header className="queue-head">
        <h2 className="queue-title">Up next</h2>
        <button
          type="button"
          className="queue-close"
          onClick={closeQueue}
          aria-label="Close queue"
        >
          <Glyph name="close" />
        </button>
      </header>

      {queue.length === 0 ? (
        <p className="queue-empty">The queue is empty.</p>
      ) : (
        <ol className="queue-list">
          {queue.map((track, position) => {
            const isCurrent = position === index;

            return (
              <li
                key={`${track.id}-${position}`}
                className={[
                  "queue-item",
                  isCurrent ? "queue-item--current" : "",
                  draggingIndex === position ? "queue-item--dragging" : "",
                  dropIndex === position && draggingIndex !== position
                    ? "queue-item--dropzone"
                    : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
                draggable
                onDragStart={(event) => onDragStart(event, position)}
                onDragOver={(event) => onDragOver(event, position)}
                onDrop={(event) => onDrop(event, position)}
                onDragEnd={onDragEnd}
              >
                <span className="queue-handle" aria-hidden="true">
                  <Glyph name="drag" />
                </span>

                <button
                  type="button"
                  className="queue-play"
                  onClick={() => goTo(position)}
                  aria-current={isCurrent ? "true" : undefined}
                >
                  <span
                    className="queue-cover"
                    style={
                      track.artworkUrl
                        ? { backgroundImage: `url(${track.artworkUrl})` }
                        : undefined
                    }
                  />
                  <span className="queue-meta">
                    <span className="queue-name">{track.title}</span>
                    <span className="queue-artist">{track.artistName}</span>
                  </span>
                </button>

                <span className="queue-duration">
                  {formatTime((track.durationMs ?? 0) / 1000)}
                </span>

                <button
                  type="button"
                  className="queue-remove"
                  onClick={() => removeFromQueue(position)}
                  aria-label={`Remove ${track.title} from the queue`}
                >
                  <Glyph name="close" />
                </button>
              </li>
            );
          })}
        </ol>
      )}
    </aside>
  );
}
