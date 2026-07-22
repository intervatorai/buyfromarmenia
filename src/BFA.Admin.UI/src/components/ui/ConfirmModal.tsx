"use client";

import { Modal } from "./Modal";

type ConfirmModalProps = {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  danger?: boolean;
  onClose: () => void;
  onConfirm: () => void | Promise<void>;
};

export function ConfirmModal({
  open,
  title,
  message,
  confirmLabel = "Confirm",
  danger,
  onClose,
  onConfirm,
}: ConfirmModalProps) {
  return (
    <Modal
      open={open}
      title={title}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="button-ghost" onClick={onClose}>
            Cancel
          </button>
          <button
            type="button"
            className={danger ? "button-primary" : "button-secondary"}
            onClick={() => void onConfirm()}
          >
            {confirmLabel}
          </button>
        </>
      }
    >
      <p style={{ margin: 0, color: "var(--admin-muted)", lineHeight: 1.5 }}>{message}</p>
    </Modal>
  );
}
