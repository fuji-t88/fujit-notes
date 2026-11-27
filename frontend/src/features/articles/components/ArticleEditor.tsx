
"use client";

import { EditorContent, useEditor, useEditorState } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";

type ArticleEditorProps = {
  initialContent?: string;
  onChange: (html: string) => void;
};

export default function ArticleEditor({
  initialContent = "",
  onChange,
}: ArticleEditorProps) {
  const editor = useEditor({
    extensions: [StarterKit],
    content: initialContent,
    immediatelyRender: false,
    editorProps: {
      attributes: {
        class:
          "min-h-[360px] px-5 py-4 focus:outline-none " +
          "text-gray-800 leading-8 break-words " +
          "[&_h1]:text-3xl [&_h1]:font-bold " +
          "[&_h2]:text-2xl [&_h2]:font-bold " +
          "[&_h3]:text-xl [&_h3]:font-bold " +
          "[&_p]:my-3 " +
          "[&_ul]:list-disc [&_ul]:pl-6 " +
          "[&_ol]:list-decimal [&_ol]:pl-6 " +
          "[&_blockquote]:border-l-4 " +
          "[&_blockquote]:border-orange-400 " +
          "[&_blockquote]:pl-4 " +
          "[&_pre]:overflow-x-auto " +
          "[&_pre]:rounded-lg " +
          "[&_pre]:bg-gray-900 " +
          "[&_pre]:p-4 " +
          "[&_pre]:text-gray-100",
      },
    },
    onUpdate: ({ editor }) => {
      onChange(editor.getHTML());
    },
  });

  const state = useEditorState({
    editor,
    selector: ({ editor }) => ({
      bold: editor?.isActive("bold") ?? false,
      italic: editor?.isActive("italic") ?? false,
      heading1: editor?.isActive("heading", { level: 1 }) ?? false,
      heading2: editor?.isActive("heading", { level: 2 }) ?? false,
      bulletList: editor?.isActive("bulletList") ?? false,
      orderedList: editor?.isActive("orderedList") ?? false,
      codeBlock: editor?.isActive("codeBlock") ?? false,
    }),
  });

  if (!editor) {
    return (
      <div className="rounded-lg border border-gray-300 p-5 text-gray-500">
        エディタを準備しています...
      </div>
    );
  }

  const buttons = [
    {
      label: "H1",
      active: state?.heading1,
      action: () =>
        editor.chain().focus().toggleHeading({ level: 1 }).run(),
    },
    {
      label: "H2",
      active: state?.heading2,
      action: () =>
        editor.chain().focus().toggleHeading({ level: 2 }).run(),
    },
    {
      label: "太字",
      active: state?.bold,
      action: () => editor.chain().focus().toggleBold().run(),
    },
    {
      label: "斜体",
      active: state?.italic,
      action: () => editor.chain().focus().toggleItalic().run(),
    },
    {
      label: "箇条書き",
      active: state?.bulletList,
      action: () => editor.chain().focus().toggleBulletList().run(),
    },
    {
      label: "番号付き",
      active: state?.orderedList,
      action: () => editor.chain().focus().toggleOrderedList().run(),
    },
    {
      label: "コード",
      active: state?.codeBlock,
      action: () => editor.chain().focus().toggleCodeBlock().run(),
    },
  ];

  return (
    <div className="overflow-hidden rounded-xl border border-gray-300 bg-white">
      <div className="flex flex-wrap gap-2 border-b border-gray-200 bg-gray-50 p-3">
        {buttons.map((button) => (
          <button
            key={button.label}
            type="button"
            onClick={button.action}
            aria-pressed={Boolean(button.active)}
            className={
              "rounded-md border px-3 py-2 text-sm font-semibold transition " +
              (button.active
                ? "border-orange-500 bg-orange-100 text-orange-700"
                : "border-gray-200 bg-white text-gray-700 hover:bg-gray-100")
            }
          >
            {button.label}
          </button>
        ))}

        <button
          type="button"
          onClick={() => editor.chain().focus().undo().run()}
          disabled={!editor.can().undo()}
          className="rounded-md border border-gray-200 bg-white px-3 py-2 text-sm disabled:opacity-40"
        >
          元に戻す
        </button>

        <button
          type="button"
          onClick={() => editor.chain().focus().redo().run()}
          disabled={!editor.can().redo()}
          className="rounded-md border border-gray-200 bg-white px-3 py-2 text-sm disabled:opacity-40"
        >
          やり直す
        </button>
      </div>

      <EditorContent editor={editor} />
    </div>
  );
}
