import { DEFAULT_ACCENT, accentLift } from "@utils/dominantColor";
import "@css/AccentPattern.css";

const classes = (...names) => names.filter(Boolean).join(" ");

export default function AccentPattern({
  image,
  accent,
  className,
  imageClassName,
  children,
}) {
  return (
    <div
      className={classes("accent-pattern", className)}
      style={{ "--accent-lift": accentLift(accent) }}
    >
      <img
        className={classes("accent-pattern__asset", imageClassName)}
        src={image}
        alt=""
        aria-hidden="true"
      />
      <div
        className="accent-pattern__tint"
        style={{
          backgroundColor: accent || DEFAULT_ACCENT,
          opacity: accent ? 1 : 0,
        }}
      />
      {children}
    </div>
  );
}
