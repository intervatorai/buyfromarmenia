"use client";

import {
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
  type KeyboardEvent,
} from "react";
import { useGoogleMaps } from "@/hooks/useGoogleMaps";
import {
  geocodeAddressString,
  parseGooglePlace,
  searchAddressSuggestions,
  type AddressSuggestion,
} from "@/lib/address-autocomplete";

type AddressAutocompleteProps = {
  value: string;
  onChange: (value: string) => void;
  onSelect: (suggestion: AddressSuggestion) => void;
  /** ISO-2 codes from admin-enabled shipping countries. */
  countryCodes: string[];
  /** Prefer restricting Google Places to this country (selected in the form). */
  preferredCountryCode?: string;
  placeholder?: string;
  disabled?: boolean;
  required?: boolean;
};

export function AddressAutocomplete({
  value,
  onChange,
  onSelect,
  countryCodes,
  preferredCountryCode,
  placeholder = "Start typing an address…",
  disabled = false,
  required = false,
}: AddressAutocompleteProps) {
  const listId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const autocompleteRef = useRef<google.maps.places.Autocomplete | null>(null);
  const onSelectRef = useRef(onSelect);
  const onChangeRef = useRef(onChange);
  const { apiLoaded, hasKey } = useGoogleMaps();

  const [suggestions, setSuggestions] = useState<AddressSuggestion[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [open, setOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);

  const countryCodesKey = useMemo(
    () => countryCodes.map((code) => code.toUpperCase()).join(","),
    [countryCodes],
  );

  useEffect(() => {
    onSelectRef.current = onSelect;
    onChangeRef.current = onChange;
  }, [onSelect, onChange]);

  const restrictionCountry =
    preferredCountryCode ||
    (countryCodes.length === 1 ? countryCodes[0] : undefined);

  function applySuggestion(suggestion: AddressSuggestion) {
    const line1 = suggestion.line1 || suggestion.displayName;
    onChangeRef.current(line1);
    if (inputRef.current) {
      inputRef.current.value = line1;
    }
    onSelectRef.current(suggestion);
    setSuggestions([]);
    setOpen(false);
    setActiveIndex(-1);
  }

  // Google Places Autocomplete — Zentbow pattern with retries + geocode fallback.
  useEffect(() => {
    if (!hasKey || !apiLoaded || !inputRef.current || disabled) {
      return;
    }

    if (autocompleteRef.current) {
      google.maps.event.clearInstanceListeners(autocompleteRef.current);
      autocompleteRef.current = null;
    }

    const options: google.maps.places.AutocompleteOptions = {
      fields: ["address_components", "formatted_address", "geometry", "name"],
      types: ["address"],
    };

    if (restrictionCountry) {
      options.componentRestrictions = {
        country: restrictionCountry.toLowerCase(),
      };
    } else if (countryCodes.length > 0 && countryCodes.length <= 5) {
      options.componentRestrictions = {
        country: countryCodes.map((code) => code.toLowerCase()),
      };
    }

    const autocomplete = new google.maps.places.Autocomplete(
      inputRef.current,
      options,
    );
    autocompleteRef.current = autocomplete;

    const processPlace = async (place: google.maps.places.PlaceResult) => {
      let suggestion = parseGooglePlace(place);
      if (!suggestion && inputRef.current?.value) {
        suggestion = await geocodeAddressString(
          inputRef.current.value,
          restrictionCountry,
        );
      }
      if (!suggestion) {
        return;
      }
      applySuggestion(suggestion);
    };

    const handlePlaceChanged = () => {
      const place = autocomplete.getPlace();
      if (place?.address_components?.length || place?.geometry) {
        void processPlace(place);
        return;
      }

      // Google sometimes fires before place details are ready (same as Zentbow).
      let attempts = 0;
      const retry = () => {
        attempts += 1;
        const next = autocomplete.getPlace();
        if (next?.address_components?.length || next?.geometry) {
          void processPlace(next);
          return;
        }
        if (attempts < 5) {
          window.setTimeout(retry, 200);
          return;
        }
        if (inputRef.current?.value) {
          void geocodeAddressString(inputRef.current.value, restrictionCountry).then(
            (suggestion) => {
              if (suggestion) {
                applySuggestion(suggestion);
              }
            },
          );
        }
      };
      window.setTimeout(retry, 200);
    };

    const listener = autocomplete.addListener("place_changed", handlePlaceChanged);

    return () => {
      google.maps.event.removeListener(listener);
      if (autocompleteRef.current) {
        google.maps.event.clearInstanceListeners(autocompleteRef.current);
        autocompleteRef.current = null;
      }
    };
    // countryCodesKey keeps the effect stable across parent re-renders.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hasKey, apiLoaded, disabled, restrictionCountry, countryCodesKey]);

  // Nominatim fallback when Google key is missing.
  useEffect(() => {
    if (hasKey || disabled) {
      return;
    }

    const query = value.trim();
    if (query.length < 3) {
      setSuggestions([]);
      setOpen(false);
      return;
    }

    const handle = window.setTimeout(() => {
      void (async () => {
        setIsSearching(true);
        try {
          const codes = preferredCountryCode
            ? [preferredCountryCode]
            : countryCodes;
          const results = await searchAddressSuggestions(query, codes);
          setSuggestions(results);
          setOpen(results.length > 0);
          setActiveIndex(-1);
        } catch {
          setSuggestions([]);
          setOpen(false);
        } finally {
          setIsSearching(false);
        }
      })();
    }, 350);

    return () => window.clearTimeout(handle);
  }, [value, hasKey, disabled, countryCodes, preferredCountryCode]);

  function pickSuggestion(suggestion: AddressSuggestion) {
    applySuggestion(suggestion);
  }

  function onKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (!open || suggestions.length === 0) {
      return;
    }

    if (event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((current) => (current + 1) % suggestions.length);
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((current) =>
        current <= 0 ? suggestions.length - 1 : current - 1,
      );
    } else if (event.key === "Enter" && activeIndex >= 0) {
      event.preventDefault();
      pickSuggestion(suggestions[activeIndex]);
    } else if (event.key === "Escape") {
      setOpen(false);
    }
  }

  useEffect(() => {
    if (!hasKey || !inputRef.current) {
      return;
    }
    if (document.activeElement !== inputRef.current && inputRef.current.value !== value) {
      inputRef.current.value = value;
    }
  }, [hasKey, value]);

  return (
    <div className="address-autocomplete">
      <input
        ref={inputRef}
        type="text"
        required={required}
        disabled={disabled}
        {...(hasKey
          ? { defaultValue: value }
          : {
              value,
              onChange: (event: ChangeEvent<HTMLInputElement>) =>
                onChange(event.target.value),
            })}
        placeholder={placeholder}
        autoComplete="off"
        role="combobox"
        aria-expanded={open}
        aria-controls={listId}
        aria-autocomplete="list"
        onKeyDown={onKeyDown}
        onBlur={() => {
          window.setTimeout(() => setOpen(false), 150);
        }}
        onFocus={() => {
          if (!hasKey && suggestions.length > 0) {
            setOpen(true);
          }
        }}
        onInput={
          hasKey
            ? (event) => {
                onChange((event.target as HTMLInputElement).value);
              }
            : undefined
        }
      />

      {!hasKey && isSearching ? (
        <span className="address-autocomplete-hint">Searching…</span>
      ) : null}

      {!hasKey && open && suggestions.length > 0 ? (
        <ul id={listId} className="address-autocomplete-list" role="listbox">
          {suggestions.map((suggestion, index) => (
            <li key={`${suggestion.displayName}-${index}`} role="option">
              <button
                type="button"
                className={
                  index === activeIndex
                    ? "address-autocomplete-item active"
                    : "address-autocomplete-item"
                }
                onMouseDown={(event) => {
                  event.preventDefault();
                  pickSuggestion(suggestion);
                }}
              >
                {suggestion.displayName}
              </button>
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  );
}
