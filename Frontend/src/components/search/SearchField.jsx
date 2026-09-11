import "../../css/Search.css";

export default function SearchField({
  value,
  onChange,
  placeholder = "Search...",
  onFocus,
  onKeyDown,
  expanded,
  controls,
}) {
  return (
    <div className="search-field">
      <input
        type="search"
        className="search-field__input"
        value={value}
        placeholder={placeholder}
        aria-label="Search"
        role="combobox"
        aria-expanded={expanded ?? false}
        aria-controls={controls}
        aria-autocomplete="list"
        autoComplete="off"
        onChange={(event) => onChange(event.target.value)}
        onFocus={onFocus}
        onKeyDown={onKeyDown}
      />
      {value && (
        <button
          type="button"
          className="search-field__clear"
          aria-label="Clear search"
          onClick={() => onChange("")}
        >
          ✕
        </button>
      )}
    </div>
  );
}
