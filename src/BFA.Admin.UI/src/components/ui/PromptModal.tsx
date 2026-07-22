"use client";

import { FormEvent, useEffect, useState } from "react";
import { Modal } from "./Modal";

type PromptModalProps = {
  open: boolean;
  title: string;
  label?: string;
  initialValue?: string;
  templates?: string[];
  confirmLabel?: string;
  required?: boolean;
  onClose: () => void;
  onConfirm: (value: string) => void | Promise<void>;
};

export function PromptModal({
  open,
  title,
  label = "Reason",
  initialValue = "",
  templates = [],
  confirmLabel = "Confirm",
  required = true,
  onClose,
  onConfirm,
}: PromptModalProps) {
  const [value, setValue] = useState(initialValue);
  const [template, setTemplate] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (open) {
      setValue(initialValue);
      setTemplate("");
      setError("");
      setIsSaving(false);
    }
  }, [open, initialValue]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (required && !value.trim()) {
      setError(`${label} is required.`);
      return;
    }

    setIsSaving(true);
    setError("");
    try {
      await onConfirm(value.trim());
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Action failed.");
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open={open}
      title={title}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="button-ghost" onClick={onClose} disabled={isSaving}>
            Cancel
          </button>
          <button
            type="submit"
            form="prompt-modal-form"
            className="button-primary"
            disabled={isSaving}
          >
            {isSaving ? "Working..." : confirmLabel}
          </button>
        </>
      }
    >
      <form id="prompt-modal-form" onSubmit={(event) => void handleSubmit(event)}>
        {error ? <p className="form-error">{error}</p> : null}
        {templates.length > 0 ? (
          <div className="form-field">
            <label htmlFor="prompt-template">Template</label>
            <select
              id="prompt-template"
              className="form-control"
              value={template}
              onChange={(event) => {
                const next = event.target.value;
                setTemplate(next);
                if (next) {
                  setValue(next);
                }
              }}
            >
              <option value="">Custom…</option>
              {templates.map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </div>
        ) : null}
        <div className="form-field">
          <label htmlFor="prompt-value">{label}</label>
          <textarea
            id="prompt-value"
            className="form-control"
            rows={4}
            required={required}
            value={value}
            onChange={(event) => setValue(event.target.value)}
            autoFocus
          />
        </div>
      </form>
    </Modal>
  );
}
