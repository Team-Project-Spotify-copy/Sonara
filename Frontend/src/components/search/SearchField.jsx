import "../../css/Search.css";

export default function SearchField({ value, onChange, placeholder = "Search..." }) {
  return (
    <div className="search-field">
      <input
        type="search"
        className="search-field__input"
        value={value}
        placeholder={placeholder}
        aria-label="Search"
        onChange={(event) => onChange(event.target.value)}
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
