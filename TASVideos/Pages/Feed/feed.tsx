import { useState, useEffect, useCallback, useRef, useLayoutEffect } from "react";
import { createRoot } from "react-dom/client";
import TextareaAutosize from 'react-textarea-autosize';
import Markdown from 'react-markdown';
import { Components } from "react-markdown";
import remarkGfm from 'remark-gfm';

async function postForm(handler: string, params: URLSearchParams): Promise<unknown> {
	maybeRedirectToLogin();
	const token = (document.querySelector('input[name="__RequestVerificationToken"]') as HTMLInputElement | null)?.value ?? null;
	const headers: Record<string, string> = { "Content-Type": "application/x-www-form-urlencoded" };
	if (token) { headers["RequestVerificationToken"] = token; }

	const res = await fetch(`/Feed?handler=${handler}`, {
		method: "POST",
		headers,
		body: params.toString()
	});

	if (!res.ok) {
		const txt = await res.text().catch(() => "");
		throw new Error(txt);
	}

	const json = await res.json();
	const jsonAchievement = json as AchievementResult<unknown>;
	if (jsonAchievement && jsonAchievement.newAchievements) {
		handleAchievements(jsonAchievement.newAchievements);
		return jsonAchievement.data;
	}
	return json;
}

function scrollToTop() {
	window.scrollTo({ top: 0, behavior: "smooth" });
}


function handleAchievements(achievements: Achievement[]) {
	if (achievements?.length) {
		window.dispatchEvent(new CustomEvent("newAchievements", { detail: achievements }));
	}
}

function achievementKeyToClass(key: string, tier: number): string {
	const index = ["UpvotesMade", "ReactionsMade", "CommentsCreated", "PostsCreated", "HighScoreReached", "DownvotesMade"].indexOf(key);
	if (index !== -1) {
		return `feedtile-${index}-${tier - 1}`;
	}
	switch (key) {
		case "DownvoteSelf": return "feedtile-6-0";
		case "ReplyToOwnComment": return "feedtile-6-1";
		case "ThreadDepth5": return "feedtile-6-2";
		case "LongContent": return "feedtile-7-0";
		case "UndoReaction": return "feedtile-7-1";
		default: return "feedtile-0-0";
	}
}

function achievementKeyToDetails(
	key: string,
	tier: number,
	isAchieved: boolean
): { title: string; requirement: string } {
	const hidden = !isAchieved;

	switch (key) {
		case "UpvotesMade":
			return {
				title: ["First Boost", "Signal Amplifier", "Hype Machine"][tier - 1],
				requirement: `Give ${[1, 20, 50][tier - 1]} upvote${[1, 20, 50][tier - 1] === 1 ? "" : "s"}.`,
			};

		case "ReactionsMade":
			return {
				title: ["Express Yourself", "Crowd Energizer", "Reaction Champion"][tier - 1],
				requirement: `React ${[1, 7, 30][tier - 1]} time${[1, 7, 30][tier - 1] === 1 ? "" : "s"}.`,
			};

		case "CommentsCreated":
			return {
				title: ["First Word", "Chatterbox", "Conversationalist"][tier - 1],
				requirement: `Create ${[1, 4, 10][tier - 1]} comment${[1, 4, 10][tier - 1] === 1 ? "" : "s"}.`,
			};

		case "PostsCreated":
			return {
				title: ["Newcomer", "Contributor", "Trendsetter"][tier - 1],
				requirement: `Create ${[1, 3, 5][tier - 1]} post${[1, 3, 5][tier - 1] === 1 ? "" : "s"}.`,
			};

		case "HighScoreReached":
			return {
				title: ["Rising Star", "Crowd Favorite", "Viral Legend"][tier - 1],
				requirement: `Reach a score of ${[2, 4, 6][tier - 1]} on a post or comment.`,
			};

		case "DownvotesMade":
			return {
				title: ["Skeptic", "Critic", "Contrarian"][tier - 1],
				requirement: `Give ${[1, 2, 3][tier - 1]} downvote${[1, 2, 3][tier - 1] === 1 ? "" : "s"}.`,
			};

		case "DownvoteSelf":
			return {
				title: hidden ? "???" : "My Worst Enemy",
				requirement: hidden ? "???" : "Downvote your own post or comment.",
			};

		case "ReplyToOwnComment":
			return {
				title: hidden ? "???" : "Inner Dialogue",
				requirement: hidden ? "???" : "Reply to yourself.",
			};

		case "ThreadDepth5":
			return {
				title: hidden ? "???" : "Down the Rabbit Hole",
				requirement: hidden ? "???" : "Make a comment in a thread of depth 5 or higher.",
			};

		case "LongContent":
			return {
				title: hidden ? "???" : "Novelist",
				requirement: hidden ? "???" : "Create a comment or post with at least 500 characters.",
			};

		case "UndoReaction":
			return {
				title: hidden ? "???" : "On Second Thought",
				requirement: hidden ? "???" : "Undo a reaction.",
			};

		default:
			return { title: "Unknown", requirement: "" };
	}
}

const ALL_TIERED_ACHIEVEMENTS = ["UpvotesMade", "ReactionsMade", "CommentsCreated", "PostsCreated", "HighScoreReached", "DownvotesMade"];
const ALL_SPECIAL_ACHIEVEMENTS = ["DownvoteSelf", "ReplyToOwnComment", "ThreadDepth5", "LongContent", "UndoReaction"];

async function getFetch(handler: string, params?: URLSearchParams): Promise<unknown> {
	const res = await fetch(`/Feed?handler=${handler}${params ? "&" + params.toString() : ""}`);
	if (!res.ok) {
		const txt = await res.text().catch(() => "");
		throw new Error(txt);
	}

	return await res.json();
}

function maybeRedirectToLogin() {
	if (!isLoggedIn()) {
		window.location.href = "/Account/Login?returnUrl=" + encodeURIComponent(location.pathname + location.search);
		throw new Error("Please log in.");
	}
}

type Achievement = {
	id: number;
	hasSeen: boolean;
	key: string;
	tier: number;
}

type AchievementResult<T> = {
	data: T;
	newAchievements: Achievement[];
}

type ContentType = "Text" | "Image" | "Submission" | "Publication" | "Activity";

type ReactionEmoji = "👍" | "👎" | "😄" | "😕" | "❤️" | "🎉" | "🚀" | "👀";
const ALL_REACTIONS: ReactionEmoji[] = ["👍", "👎", "😄", "😕", "❤️", "🎉", "🚀", "👀"];

type VoteState = 1 | -1 | 0;

interface ReactionData {
	count: number;
	myReaction: boolean;
}

interface Comment {
	id: string;
	postId: string;
	postTitle: string | null;
	parentId: string | null;
	author: string | null;
	authorAvatar: string | null;
	date: string;
	lastEdited: string | null;
	content: string;
	score: number | null;
	myVote: VoteState;
	reactions: Record<string, ReactionData>;
	isDeleted: boolean;
	replies: Comment[];
}

interface Post {
	id: string;
	author: string | null;
	authorAvatar: string | null;
	date: string;
	lastEdited: string | null;
	title: string;
	contentType: ContentType;
	content: string;
	extraVideoContent: string;
	extraOverrideLink: string;
	score: number | null;
	myVote: VoteState;
	reactions: Record<string, ReactionData>;
	isDeleted: boolean;
	isSticky: boolean;
	comments: Comment[];
}

function feedPostTitleToUrl(postId: string, postTitle: string | null | undefined, commentId: string | null | undefined = null): string {
	const commentPart = commentId ? `/${commentId}` : "";
	if (!postTitle) { return `/Feed/${postId}/_${commentPart}`; }

	let newTitle: string = postTitle
		.toLowerCase()
		.replace(/[^a-z0-9\s]/g, "")
		.replace(/\s+/g, "_");

	if (newTitle.length > 50) {
		const firstPart: string = newTitle.substring(0, 51);

		if (firstPart.includes("_")) {
			newTitle = newTitle.substring(0, firstPart.lastIndexOf("_"));
		} else {
			newTitle = newTitle.substring(0, 50);
		}
	}

	return `/Feed/${postId}/${newTitle.length === 0 ? "_" : newTitle}${commentPart}`;
}

function now() {
	return new Date().toISOString();
}

type PageType = "Home" | "Post" | "Comment" | "My";
interface Page {
	userName?: string;
	canModEdit: boolean;
	canModDelete: boolean;
	type: PageType;
	post?: Post;
	commentId?: string
}

function formatDate(iso: string) {
	iso = iso.endsWith("Z") ? iso : iso + "Z";
	const s = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);

	if (s < 5) { return "Now"; }

	const units: [number, string][] = [
		[86400, 'd'],
		[3600, 'h'],
		[60, 'm'],
		[1, 's']
	];

	for (const [unit, label] of units) {
		if (s >= unit) { return Math.floor(s / unit) + label + ' ago'; }
	}
}

function getScore(score: number | null) {
	if (score === null) { return <span>·</span>; }
	return <span>{score}</span>;
}

function canEdit(author: string | null) {
	return initState.canModEdit || initState.userName === author;
}

function canDelete(author: string | null) {
	return initState.canModDelete || initState.userName === author;
}

function isLoggedIn() {
	return !!initState.userName;
}

function updateComment(comments: Comment[], id: string, updater: (c: Comment) => Comment): Comment[] {
	return comments.map((c) => {
		if (c.id === id) { return updater(c); }
		return { ...c, replies: updateComment(c.replies, id, updater) };
	});
}

function ReactionPicker({
	reactions, onReact, isOnComment
}: {
	reactions: Record<string, ReactionData>;
	onReact: (emoji: ReactionEmoji) => void;
	isOnComment: boolean;
}) {
	const [open, setOpen] = useState(false);

	const borderClass = isOnComment ? "" : " border";
	const paddingClass = isOnComment ? " px-1" : " px-2"
	const colorClass = isOnComment ? " text-body-secondary" : "";

	return (
		<div className="d-flex gap-1 flex-wrap">
			<div className={"btn rounded-5 py-1 d-flex align-items-center" + borderClass + paddingClass + colorClass} onClick={() => setOpen((v) => !v)}>
				<i className="fa-regular fa-face-smile"></i>
				{open && (
					<span className="ms-2">
						{ALL_REACTIONS.map((e) => (
							<span
								key={e}
								onClick={() => { onReact(e); }}
								style={{ cursor: "pointer", opacity: reactions[e]?.myReaction ? 1 : 0.5 }}
							>
								{e}
							</span>
						))}
					</span>
				)}
			</div>
			{ALL_REACTIONS.filter((e) => reactions[e]?.count).map((e) => (
				<div className={"btn rounded-5 py-1 d-flex align-items-center" + borderClass + paddingClass + colorClass} key={e} onClick={() => onReact(e)} style={{ fontWeight: reactions[e].myReaction ? "bold" : "normal" }}>
					{e}{" "}{reactions[e].count}
				</div>
			))}
		</div>
	);
}

function VoteButtons({
	score, myVote, onVote, isOnComment
}: {
	score: number | null;
	myVote: VoteState;
	onVote: (v: 1 | -1) => void;
	isOnComment: boolean;
}) {
	return (
		isOnComment ? (
			<div className="rounded-5 py-1 d-flex align-items-center text-body-secondary">
				<button onClick={() => onVote(1)} className="btn rounded-5 py-0 ps-0 pe-2">
					{myVote === 1 ? "▲" : "△"}
				</button>
				{getScore(score)}
				<button onClick={() => onVote(-1)} className="btn rounded-5 py-0 px-2">
					{myVote === -1 ? "▼" : "▽"}
				</button>
			</div>
		) : (
			<div className={"border rounded-5 py-1 d-flex align-items-center" + (myVote === 1 ? " bg-publication" : myVote === -1 ? " bg-danger-subtle" : " bg-body")}>
				<button onClick={() => onVote(1)} className="btn rounded-5 py-0 px-2">
					{myVote === 1 ? "▲" : "△"}
				</button>
				{getScore(score)}
				<button onClick={() => onVote(-1)} className="btn rounded-5 py-0 px-2">
					{myVote === -1 ? "▼" : "▽"}
				</button>
			</div>
		)

	);
}

type OverflowWrapperProps = {
	children: React.ReactNode;
	maxHeight?: number; // px
	viewFullHref: string;
};

const OverflowWrapper: React.FC<OverflowWrapperProps> = ({
	children,
	maxHeight = 300,
	viewFullHref,
}) => {
	const ref = useRef<HTMLDivElement | null>(null);
	const [isOverflowing, setIsOverflowing] = useState(false);

	useLayoutEffect(() => {
		const el = ref.current;
		if (!el) { return; }

		const check = () => {
			// eslint-disable-next-line @eslint-react/set-state-in-effect
			setIsOverflowing(el.scrollHeight > el.clientHeight);
		};

		check();

		const observer = new ResizeObserver(check);
		observer.observe(el);

		return () => observer.disconnect();
	}, []);

	return (
		<div>
			<div
				ref={ref}
				style={{
					maxHeight,
					overflow: "hidden",
				}}
			>
				{children}
			</div>

			{isOverflowing && (
				<div className="mt-2 d-flex">
					<a href={viewFullHref} className="border p-2 bg-body-secondary flex-grow-1">
						Read full post
					</a>
				</div>
			)}
		</div>
	);
};

interface FeedMarkdownProps {
	children: string;
}

const FeedMarkdown: React.FC<FeedMarkdownProps> = ({ children }) => {
	const components: Components = {
		table: ({ ...props }) => <table className="table table-sm table-bordered table-striped" {...props} />,
		img: ({ ...props }) => <img className="img-fluid" {...props} />
	};

	return <Markdown components={components} remarkPlugins={[remarkGfm]}>{children}</Markdown>;
}

function PostContent({ post, isSinglePost }: { post: Post, isSinglePost: boolean }) {
	if (post.contentType === "Text") {
		return isSinglePost
			? <FeedMarkdown>{post.content}</FeedMarkdown>
			: <OverflowWrapper viewFullHref={feedPostTitleToUrl(post.id, post.title)}>
				<FeedMarkdown>{post.content}</FeedMarkdown>
			</OverflowWrapper>;
	}
	if (post.contentType === "Image") {
		return <a className="d-flex justify-content-center border rounded-2" style={isSinglePost ? undefined : { maxWidth: "542px" }} href={feedPostTitleToUrl(post.id, post.title)}>
			<img src={post.content} className="img-fluid rounded-2" style={{ minHeight: "220px", ...(isSinglePost ? {} : { maxHeight: "540px" }) }} />
		</a>;
	}
	if (post.contentType === "Submission" || post.contentType === "Publication") {
		return <>
			<a href={post.extraOverrideLink} className="text-decoration-none text-body"><div>{post.content}</div></a>
			{post.extraVideoContent &&
				<div className="d-flex my-2" style={{ height: "390px" }}>
					<iframe src={post.extraVideoContent} allowFullScreen={true} className="mw-100" style={{ aspectRatio: "16 / 9" }}></iframe>
				</div >
			}
		</>;
	}
	return null;
}

function CreatePostForm({ onCreated }: { onCreated: (post: Post) => void }) {
	const [title, setTitle] = useState("");
	const [contentType, setContentType] = useState<ContentType>("Text");
	const [content, setContent] = useState("");
	const [preview, setPreview] = useState<{ title: string; contentType: ContentType; content: string } | null>(null);
	const [error, setError] = useState<string | null>(null);
	const [submitting, setSubmitting] = useState(false);

	async function submit() {
		if (!title.trim()) { setError("Title is required."); return; }
		setError(null);
		setSubmitting(true);
		try {
			const params = new URLSearchParams({ title, contentType, content });
			const result = await postForm("PostCreate", params) as Post;
			onCreated(result);
		} catch (e) {
			setError(String(e instanceof Error ? e.message : e));
		} finally {
			setSubmitting(false);
		}
	}

	const previewPost: Post | null = preview
		? { id: "__preview__", author: null, authorAvatar: null, date: now(), lastEdited: null, ...preview, score: 0, myVote: 0, reactions: {}, isDeleted: false, isSticky: false, comments: [], extraOverrideLink: "", extraVideoContent: "" }
		: null;

	return (
		<div className="my-2">
			<h3>Create Post</h3>
			<div className="form-floating mb-3">
				<input
					id="floatingTitle"
					className="form-control"
					placeholder="Title"
					value={title}
					onChange={(e) => setTitle(e.target.value)}
					required
				/>
				<label htmlFor="floatingTitle">Title<span style={{ color: "red" }} >*</span></label>
			</div>
			<div className="d-flex gap-2 my-1">
				{(["Text", "Image"] as ContentType[]).map((t) => (
					<span key={t}>
						<input
							className="btn-check"
							type="radio"
							id={"newpost" + t}
							checked={contentType === t}
							onChange={() => setContentType(t)} />
						<label className="btn btn-outline-silver" htmlFor={"newpost" + t}>{t}</label>
					</span>
				))}
			</div>
			<div>
				{contentType === "Text"
					? <TextareaAutosize
						className="form-control"
						placeholder="Body Text (optional)"
						value={content}
						onChange={(e) => setContent(e.target.value)}
						minRows={3} />
					:
					<div className="form-floating mb-3">
						<input
							id="floatingContent"
							className="form-control"
							placeholder="Image URL"
							value={content}
							onChange={(e) => setContent(e.target.value)}
							required
						/>
						<label htmlFor="floatingContent">Image URL<span style={{ color: "red" }} >*</span></label>
					</div>}
			</div>
			{error && <div className="alert alert-danger my-2">{error}</div>}
			<div className="d-flex gap-2 my-2">
				<button className="btn btn-primary" disabled={submitting} onClick={submit}><i className="fa fa-plus"></i> Post</button>
				<button className="btn btn-secondary" onClick={() => setPreview({ title, contentType, content })}><i className="fa fa-eye"></i> Preview</button>
			</div>
			{previewPost && (
				<div className="my-2 p-2 bg-body-tertiary">
					<div><strong>{previewPost.title}</strong></div>
					<PostContent post={previewPost} isSinglePost={false} />
				</div>
			)}
		</div>
	);
}

function EditPostForm({ post, onSaved, onCancel }: { post: Post; onSaved: (content: string) => void; onCancel: () => void }) {
	const [content, setContent] = useState(post.content);
	const [preview, setPreview] = useState<string | null>(null);
	const [error, setError] = useState<string | null>(null);
	const [submitting, setSubmitting] = useState(false);

	async function submit() {
		setError(null);
		setSubmitting(true);
		try {
			const params = new URLSearchParams({ id: post.id, content });
			await postForm("PostEdit", params);
			onSaved(content);
		} catch (e) {
			setError(String(e instanceof Error ? e.message : e));
		} finally {
			setSubmitting(false);
		}
	}

	return (
		<div>
			<div><TextareaAutosize className="form-control" value={content} onChange={(e) => setContent(e.target.value)} autoFocus /></div>
			{error && <div className="alert alert-danger my-2">{error}</div>}
			{content && <div className="d-flex gap-2 my-2">
				<div className={"btn btn-primary" + (submitting ? " disabled" : "")} onClick={submit}><i className="fa fa-save"></i> Save</div>
				<div className="btn btn-secondary" onClick={() => setPreview(content)}><i className="fa fa-eye"></i> Preview</div>
				<div className="btn btn-secondary" onClick={() => { setContent(""); setPreview(null); onCancel(); }}>Cancel</div>
			</div>}
			{preview !== null && <div className="my-2 p-2 bg-body-tertiary"><FeedMarkdown>{preview}</FeedMarkdown></div>}
		</div>
	);
}

function CreateCommentForm({
	postId, parentId, onCreated, onCancel, autoFocus = false
}: {
	postId: string; parentId: string | null;
	onCreated: (c: Comment) => void; onCancel: () => void;
	autoFocus?: boolean;
}) {
	const [content, setContent] = useState("");
	const [preview, setPreview] = useState<string | null>(null);
	const [error, setError] = useState<string | null>(null);
	const [submitting, setSubmitting] = useState(false);

	async function submit() {
		if (!content.trim()) { setError("Comment cannot be empty."); return; }
		setError(null);
		setPreview(null);
		setSubmitting(true);
		try {
			const params = new URLSearchParams({ postId, parentId: parentId ?? "", content });
			const result = await postForm("CommentCreate", params) as Comment;
			setContent("");
			onCreated(result);
		} catch (e) {
			setError(String(e instanceof Error ? e.message : e));
		} finally {
			setSubmitting(false);
		}
	}

	return (
		<div>
			<div className="mb-2"><TextareaAutosize onClick={maybeRedirectToLogin} className="form-control" value={content} onChange={(e) => setContent(e.target.value)} placeholder="Write a comment..." autoFocus={autoFocus} /></div>
			{error && <div className="alert alert-danger my-2">{error}</div>}
			{content && <div className="d-flex gap-2 my-2 flex-wrap">
				<div className={"btn btn-primary" + (submitting ? " disabled" : "")} onClick={submit}><i className="fa fa-plus"></i> Comment</div>
				<div className="btn btn-secondary" onClick={() => setPreview(content)}><i className="fa fa-eye"></i> Preview</div>
				<div className="btn btn-secondary" onClick={() => { setContent(""); setPreview(null); onCancel(); }}>Cancel</div>
			</div>}
			{preview !== null && <div className="my-2 p-2 bg-body-tertiary"><FeedMarkdown>{preview}</FeedMarkdown></div>}
		</div>
	);
}

function EditCommentForm({ comment, onSaved, onCancel }: { comment: Comment; onSaved: (c: string) => void; onCancel: () => void }) {
	const [content, setContent] = useState(comment.content);
	const [preview, setPreview] = useState<string | null>(null);
	const [error, setError] = useState<string | null>(null);
	const [submitting, setSubmitting] = useState(false);

	async function submit() {
		setError(null);
		setSubmitting(true);
		try {
			const params = new URLSearchParams({ id: comment.id, content });
			await postForm("CommentEdit", params);
			onSaved(content);
		} catch (e) {
			setError(String(e instanceof Error ? e.message : e));
		} finally {
			setSubmitting(false);
		}
	}

	return (
		<div>
			<div className="mb-2"><TextareaAutosize className="form-control" value={content} onChange={(e) => setContent(e.target.value)} autoFocus /></div>
			{error && <div className="alert alert-danger my-2">{error}</div>}
			{content && <div className="d-flex gap-2 my-2 flex-wrap">
				<div className={"btn btn-primary" + (submitting ? " disabled" : "")} onClick={submit}><i className="fa fa-save"></i> Save</div>
				<div className="btn btn-secondary" onClick={() => setPreview(content)}><i className="fa fa-eye"></i> Preview</div>
				<div className="btn btn-secondary" onClick={() => { setContent(""); setPreview(null); onCancel(); }}>Cancel</div>
			</div>}
			{preview !== null && <div className="my-2 p-2 bg-body-tertiary"><FeedMarkdown>{preview}</FeedMarkdown></div>}
		</div>
	);
}

function CommentNode({
	comment, updatePostComments, depth = 0, postTitle, focusedId
}: {
	comment: Comment;
	updatePostComments: (updater: (comments: Comment[]) => Comment[]) => void;
	depth?: number;
	postTitle: string | null;
	focusedId?: string;
}) {
	const [collapsed, setCollapsed] = useState(false);
	const [editing, setEditing] = useState(false);
	const [replying, setReplying] = useState(false);
	const [deleting, setDeleting] = useState(false);
	const isFocused = focusedId === comment.id;

	function vote(v: 1 | -1) {
		if (comment.isDeleted) { return; }
		updatePostComments((comments) => updateComment(comments, comment.id, (c) => {
			const delta = c.myVote === v ? -v : v - c.myVote;
			return { ...c, score: c.score == null ? null : c.score + delta, myVote: c.myVote === v ? 0 : v };
		}));
		postForm("CommentVote", new URLSearchParams({ id: comment.id, value: String(v) }));
	}

	function react(emoji: ReactionEmoji) {
		if (comment.isDeleted) { return; }
		updatePostComments((comments) => updateComment(comments, comment.id, (c) => {
			const existing = c.reactions[emoji] ?? { count: 0, myReaction: false };
			return {
				...c, reactions: {
					...c.reactions,
					[emoji]: { count: existing.myReaction ? existing.count - 1 : existing.count + 1, myReaction: !existing.myReaction },
				},
			};
		}));
		postForm("CommentReact", new URLSearchParams({ id: comment.id, emoji }));
	}

	async function deleteComment() {
		await postForm("CommentDelete", new URLSearchParams({ id: comment.id }));
		updatePostComments((comments) => updateComment(comments, comment.id, (c) => ({ ...c, isDeleted: true })));
	}

	function onReplied(newComment: Comment) {
		updatePostComments((comments) => updateComment(comments, comment.id, (c) => ({
			...c, replies: [newComment, ...c.replies],
		})));
		setReplying(false);
	}

	function countReplies(c: Comment): number {
		return c.replies.length + c.replies.reduce((sum, r) => sum + countReplies(r), 0);
	}

	const directUrl = feedPostTitleToUrl(comment.postId, postTitle, comment.id);

	return (
		<div style={depth == 0 || collapsed ? {} : { minWidth: (replying || editing ? "400px" : "250px") }} className={"d-flex flex-column gap-1 bg-body" + (depth === 0 && !collapsed ? " mb-3" : "")}>
			<div className="d-flex gap-2 align-items-center text-body-secondary flex-wrap" style={{ fontSize: "11px" }}>
				{comment.isDeleted
					? <span>[author removed]</span>
					: <AuthorImgAndName author={comment.author} imgSrc={comment.authorAvatar} />
				}
				{" · "}
				<a className="text-body-secondary" href={directUrl}>
					{formatDate(comment.date)}
					{comment.lastEdited && <span>{" (edited " + formatDate(comment.lastEdited) + ")"}</span>}
				</a>
				{" · "}
				<span onClick={() => setCollapsed((v) => !v)} style={{ cursor: "pointer" }}>
					{collapsed ? `[${1 + countReplies(comment)} more]` : "[−]"}
				</span>
			</div>

			{!collapsed && <div style={{ paddingLeft: "17px", marginLeft: "12px" }} className="border-start">
				{!editing && (
					<div {...isFocused && { className: "bg-body-secondary p-2 border border-warning-subtle" }} >
						{comment.isDeleted ? <span>[comment removed]</span> : <span><FeedMarkdown>{comment.content}</FeedMarkdown></span>}
					</div>
				)}

				{editing && !comment.isDeleted && (
					<EditCommentForm
						comment={comment}
						onSaved={(content) => {
							updatePostComments((comments) => updateComment(comments, comment.id, (c) => ({ ...c, content, lastEdited: now() })));
							setEditing(false);
						}}
						onCancel={() => setEditing(false)}
					/>
				)}

				<div className="d-flex gap-1 flex-wrap">
					<VoteButtons score={comment.score} myVote={comment.myVote} onVote={vote} isOnComment={true} />
					<div className="btn rounded-5 py-1 px-1 d-flex align-items-center gap-1 text-body-secondary" onClick={() => setReplying((v) => !v)}>
						<i className="fa fa-comment"></i><span>Reply</span>
					</div>
					<ReactionPicker reactions={comment.reactions} onReact={react} isOnComment={true} />
					{!comment.isDeleted && canEdit(comment.author) && (
						<div className="btn rounded-5 py-1 px-1 d-flex align-items-center text-body-secondary" onClick={() => setEditing((v) => !v)}><i className="fa fa-pencil"></i></div>
					)}
					{!comment.isDeleted && canDelete(comment.author) && <>
						{deleting ? (
							<div className="btn rounded-5 py-1 px-2 d-flex align-items-center text-body-secondary gap-3 border">
								<i className="fa fa-check" onClick={deleteComment} title="Delete"></i>
								<i className="fa fa-x" onClick={() => setDeleting(false)} title="Cancel"></i>
							</div>
						) : (
							<div className="btn rounded-5 py-1 px-1 d-flex align-items-center text-body-secondary" onClick={() => setDeleting(true)}><i className="fa fa-trash"></i></div>
						)}
					</>}
				</div>

				{replying && (
					<div>
						<CreateCommentForm
							postId={comment.postId}
							parentId={comment.id}
							onCreated={onReplied}
							onCancel={() => setReplying(false)}
							autoFocus={true}
						/>
					</div>
				)}

				{comment.replies.map((reply) => (
					<CommentNode
						key={reply.id}
						comment={reply}
						updatePostComments={updatePostComments}
						depth={depth + 1}
						postTitle={postTitle}
						focusedId={focusedId}
					/>
				))}
			</div>}
		</div>
	);
}

function CommentSection({
	post, updatePostComments, focusedCommentId
}: {
	post: Post;
	updatePostComments: (updater: (comments: Comment[]) => Comment[]) => void;
	hightlightCommentId?: string;
	focusedCommentId?: string;
}) {
	return (
		<div>
			{!focusedCommentId && <>
				<CreateCommentForm
					postId={post.id}
					parentId={null}
					onCreated={(c) => { updatePostComments((comments) => [...comments, c]); }}
					onCancel={() => { }}
					autoFocus={post.comments.length === 0}
				/>
				{post.comments.length === 0 && <div>No comments yet.</div>}
			</>}
			{post.comments.map((comment) => (
				<CommentNode
					key={comment.id}
					comment={comment}
					updatePostComments={updatePostComments}
					postTitle={post.title}
					focusedId={focusedCommentId}
				/>
			))}
		</div>
	);
}

function AuthorImgAndName({
	author, imgSrc
}: {
	author: string | null;
	imgSrc: string | null;
}) {
	const [imgError, setImgError] = useState(false);

	return (
		<div>{!author
			? <span>[author removed]</span>
			: <a href={`/Users/Profile/${author}`} style={{ display: "flex", alignItems: "center", gap: "5px" }} className="text-body-secondary">
				<div className="d-flex justify-content-center border rounded-5" style={{ width: "24px", height: "24px" }}>
					{imgSrc && !imgError ? (
						<img className="rounded-5" src={imgSrc} onError={() => setImgError(true)} />
					) : (
						<img className="rounded-5" src="https://gravatar.com/avatar/0?d=mp&f=y" />
					)}
				</div>
				<span>{author}</span>
			</a>
		}
		</div>
	)
}

function PostCard({
	post, setPosts, inlineComments = false, focusedCommentId, isSinglePost
}: {
	post: Post;
	setPosts: React.Dispatch<React.SetStateAction<Post[]>>;
	inlineComments?: boolean;
	focusedCommentId?: string;
	isSinglePost: boolean
}) {
	const [editing, setEditing] = useState(false);
	const [showComments, setShowComments] = useState(inlineComments);
	const [deleting, setDeleting] = useState(false);

	const directUrl = feedPostTitleToUrl(post.id, post.title);

	function countComments(comments: Comment[]): number {
		let count = comments.length;
		for (const c of comments) {
			count += countComments(c.replies);
		}
		return count;
	}

	function updatePost(updater: (p: Post) => Post) {
		setPosts((prev) => prev.map((p) => p.id === post.id ? updater(p) : p));
	}

	function updatePostComments(updater: (comments: Comment[]) => Comment[]) {
		updatePost((p) => ({ ...p, comments: updater(p.comments) }));
	}

	function vote(v: 1 | -1) {
		if (post.isDeleted) { return; }
		updatePost((p) => {
			const delta = p.myVote === v ? -v : v - p.myVote;
			return { ...p, score: p.score == null ? null : p.score + delta, myVote: p.myVote === v ? 0 : v };
		});
		postForm("PostVote", new URLSearchParams({ id: post.id, value: String(v) }));
	}

	function react(emoji: ReactionEmoji) {
		if (post.isDeleted) { return; }
		updatePost((p) => {
			const existing = p.reactions[emoji] ?? { count: 0, myReaction: false };
			return {
				...p, reactions: {
					...p.reactions,
					[emoji]: { count: existing.myReaction ? existing.count - 1 : existing.count + 1, myReaction: !existing.myReaction },
				},
			};
		});
		postForm("PostReact", new URLSearchParams({ id: post.id, emoji }));
	}

	async function deletePost() {
		await postForm("PostDelete", new URLSearchParams({ id: post.id }));
		updatePost((p) => ({ ...p, isDeleted: true }));
	}

	return (
		<div className="d-flex flex-column gap-2">
			<div className="d-flex gap-2 align-items-center text-body-secondary flex-wrap">
				<AuthorImgAndName author={post.author} imgSrc={post.authorAvatar} />
				{" · "}
				<a href={directUrl} className="text-body-secondary">
					{formatDate(post.date)}
					{post.lastEdited && <span>{" (edited " + formatDate(post.lastEdited) + ")"}</span>}
				</a>
				{post.contentType == "Submission" && <>{" · "}{<a href={post.extraOverrideLink ? post.extraOverrideLink : directUrl} className="bg-body-secondary px-1 rounded text-body-secondary">Submission</a>}</>}
				{post.contentType == "Publication" && <>{" · "}{<a href={post.extraOverrideLink ? post.extraOverrideLink : directUrl} className="bg-publication px-1 rounded text-body-secondary">Publication</a>}</>}
			</div>

			<div className={"fs-md-5 fs-6" + (post.isDeleted ? "" : " fw-bold")}>
				<a href={post.extraOverrideLink ? post.extraOverrideLink : directUrl} className={"text-body text-decoration-none" + (isSinglePost ? "" : " post-title")}>
					{post.isSticky && <><i className="text-success fa-solid fa-thumbtack"></i>{" "}</>}
					{post.isDeleted ? ("[post removed]") : post.title}
				</a>
			</div>

			{!editing && !post.isDeleted && (
				<div>
					<PostContent post={post} isSinglePost={isSinglePost} />
				</div>
			)}

			{editing && !post.isDeleted && post.contentType === "Text" && (
				<EditPostForm
					post={post}
					onSaved={(content) => {
						updatePost((p) => ({ ...p, content, lastEdited: now() }));
						setEditing(false);
					}}
					onCancel={() => setEditing(false)}
				/>
			)}

			<div className="d-flex gap-1 flex-wrap">
				<VoteButtons score={post.score} myVote={post.myVote} onVote={vote} isOnComment={false} />
				<div className="btn border rounded-5 py-1 px-2" onClick={() => setShowComments((v) => !v)}>
					<i className="fa fa-comment"></i>{!focusedCommentId && post.comments.length > 0 && " " + countComments(post.comments)}
				</div>
				<ReactionPicker reactions={post.reactions} onReact={react} isOnComment={false} />
				{!post.isDeleted && canEdit(post.author) && post.contentType === "Text" && (
					<div className="btn border rounded-5 py-1 px-2" onClick={() => setEditing((v) => !v)}><i className="fa fa-pencil"></i></div>
				)}
				{!post.isDeleted && canDelete(post.author) &&
					<>
						{deleting ? (
							<div className="btn border rounded-5 py-1 px-2 align-items-center d-flex gap-3">
								<i className="fa fa-check" onClick={deletePost} title="Delete"></i>
								<i className="fa fa-x" onClick={() => setDeleting(false)} title="Cancel"></i>
							</div>
						) : (
							<div className="btn border rounded-5 py-1 px-2" onClick={() => setDeleting(true)}><i className="fa fa-trash"></i></div>
						)}
					</>
				}
			</div>

			{focusedCommentId && (
				<div className="bg-body-tertiary p-2 my-2 d-flex flex-column">
					<span>You are viewing a single comment's thread.</span>
					<a href={directUrl}>View full post</a>
				</div>
			)}


			{showComments && <>
				<CommentSection
					post={post}
					updatePostComments={updatePostComments}
					focusedCommentId={focusedCommentId}
				/>
			</>}
		</div>
	);
}

type SortFilter = "Hot" | "New" | "Top";

async function fetchPosts(filter: SortFilter, afterId?: string): Promise<Post[]> {
	const params = new URLSearchParams({ filter });
	if (afterId) { params.set("afterId", afterId); }
	return await getFetch("Posts", params) as Post[];
}

function HomePage() {
	const [posts, setPosts] = useState<Post[]>([]);
	const [creating, setCreating] = useState(false);
	const [filter, setFilter] = useState<SortFilter>("Hot");
	const [loading, setLoading] = useState(true);
	const [noMore, setNoMore] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const lastPostIdRef = useRef<string | null>(null);
	const [reloadKey, setReloadKey] = useState(0);

	const load = useCallback(async (f: SortFilter, afterId?: string) => {
		setLoading(true);
		setNoMore(false);
		setError(null);
		try {
			const newPosts = await fetchPosts(f, afterId);
			if (newPosts.length === 0) {
				setNoMore(true);
			} else {
				lastPostIdRef.current = newPosts[newPosts.length - 1].id;
				setPosts((prev) => {
					const map = afterId
						? new Map(prev.map((post) => [post.id, post]))
						: new Map<string, Post>();
					for (const post of newPosts) {
						map.set(post.id, post);
					}
					const next = Array.from(map.values());
					return next;
				});
			}
		} catch (e) {
			setError(String(e instanceof Error ? e.message : e));
		}
		setLoading(false);
	}, []);

	function loadMore() {
		load(filter, lastPostIdRef.current ?? undefined);
	}

	useEffect(() => { load(filter); }, [load, filter, reloadKey]);

	return (
		<div>
			<div className={creating ? "" : "sticky-top"}>
				<div className="row border-bottom py-2 bg-body">
					<div className="col-12">
						<div className="d-flex">
							<button onClick={() => { maybeRedirectToLogin(); setCreating((v) => !v); scrollToTop() }} className="btn btn-primary" title="Create Post"><i className="fa fa-plus"></i></button>
							<a href="/Feed/My" className="btn btn-secondary ms-auto" title="My Posts & Comments"><i className="fa fa-user"></i></a>
						</div>
						{creating && (
							<CreatePostForm
								onCreated={(post) => { setPosts((prev) => [post, ...prev]); setCreating(false); }}
							/>
						)}
					</div>
				</div>
			</div>
			<div className="my-2 d-flex gap-2 flex-wrap">
				{(["Hot", "New", "Top"] as SortFilter[]).map((f) => (
					<button
						key={f}
						onClick={() => { setFilter(f); setPosts([]); setReloadKey(prev => prev + 1); }}
						className={"btn btn-outline-silver" + (filter === f ? " active" : "")}
					>
						{f}
					</button>
				))}
			</div>
			{error && <div className="alert alert-danger my-2">{error}</div>}
			{posts.map((post) => (
				<div key={post.id} className="row">
					<PostCard
						post={post}
						setPosts={setPosts}
						isSinglePost={false}
					/>
					<hr />
				</div>
			))}
			{loading && <div className="mb-2">Loading posts...</div>}
			{!loading && posts.length === 0 && <div className="mb-2">No posts found.</div>}
			{!loading && !noMore && posts.length > 0 && (
				<div className="mb-2">
					<button className="btn btn-outline-silver" onClick={loadMore}>
						Load more posts
					</button>
				</div>
			)}
			{noMore && posts.length > 0 && <div className="mb-2">No more posts.</div>}
		</div>
	);
}

function PostPage({
	page,
}: {
	page: Page
}) {
	const [posts, setPosts] = useState<Post[]>(page.post ? [page.post] : []);

	if (!page.post) { return <div className="my-3">Post not found.</div>; }


	return (
		<div>
			<div className="row border-bottom py-2">
				<div className="col-12">
					<a href="/Feed" className="btn btn-secondary"><i className="fa fa-home"></i></a>
				</div>
			</div>
			<div className="my-2">
				<PostCard
					post={posts[0]}
					setPosts={setPosts}
					inlineComments={true}
					isSinglePost={true}
				/>
			</div>
		</div>
	);
}

function CommentPage({
	page
}: {
	page: Page
}) {
	const [posts, setPosts] = useState<Post[]>(page.post ? [page.post] : []);

	if (!page.post) { return <div className="my-3">Comment not found.</div>; }

	return (
		<div>
			<div className="row border-bottom py-2">
				<div className="col-12">
					<a href="/Feed" className="btn btn-secondary"><i className="fa fa-home"></i></a>
				</div>
			</div>

			<div className="my-2">
				<PostCard
					post={posts[0]}
					setPosts={setPosts}
					inlineComments={true}
					focusedCommentId={page.commentId}
					isSinglePost={false}
				/>
			</div>
		</div>
	);
}

function AchivementBadge({ achievement, tier, hasAchievement, isExpanded, showNotificationBadge }: { achievement: string; tier: number; hasAchievement: boolean, isExpanded: boolean, showNotificationBadge: boolean }) {
	const details = achievementKeyToDetails(achievement, tier, hasAchievement);
	return <div className="d-flex gap-2 align-items-start">
		<div className={"position-relative feedtile-bg" + (hasAchievement ? "" : " inactive") + (tier === 1 ? " bronze" : tier === 2 ? " silver" : " gold")}>
			<div className={"feedtile " + achievementKeyToClass(achievement, tier)}></div>
			{showNotificationBadge && <div className="position-absolute top-0 start-100 translate-middle p-1 bg-danger border border-light rounded-circle"></div>}
		</div>
		{isExpanded && <div><strong>{hasAchievement ? details.title : "???"}</strong><div>{details.requirement}</div></div>}
	</div>
}

function MyPage() {
	const [myPosts, setMyPosts] = useState<Post[]>([]);
	const [myComments, setMyComments] = useState<Comment[]>([]);
	const [myAchievements, setMyAchievements] = useState<Achievement[]>([]);
	const [expandedAchievements, setExpandedAchievements] = useState(() => new Set<string>());
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);

	function toggleAchievementState(id: string) {
		setExpandedAchievements(prev => {
			const next = new Set(prev);
			if (next.has(id)) {
				next.delete(id)
			} else {
				next.add(id)
			}
			return next;
		});
	};

	useEffect(() => {
		async function load() {
			setLoading(true);
			setError(null);
			try {
				const data = await getFetch("My") as { posts: Post[]; comments: Comment[], achievements: Achievement[] };
				setMyPosts(data.posts);
				setMyComments(data.comments);
				setMyAchievements(data.achievements);
			} catch (e) {
				setError(String(e instanceof Error ? e.message : e));
			}
			setLoading(false);
		}
		load();
	}, []);

	return (
		<div>
			<div className="row border-bottom py-2">
				<div className="col-12">
					<a href="/Feed" className="btn btn-secondary"><i className="fa fa-home"></i></a>
				</div>
			</div>
			{loading && <div className="my-2">Loading...</div>}
			{error && <div className="alert alert-danger my-2">{error}</div>}
			{!loading && (
				<div className="my-2">
					<div className="my-2 d-flex flex-column gap-2">
						<h4>My Achievements ({myAchievements.length})
							{" "}{expandedAchievements.size === (ALL_TIERED_ACHIEVEMENTS.length + ALL_SPECIAL_ACHIEVEMENTS.length)
								? <span style={{ cursor: "pointer" }} onClick={() => setExpandedAchievements(new Set())}>[-]</span>
								: <span style={{ cursor: "pointer" }} onClick={() => setExpandedAchievements(new Set([...ALL_TIERED_ACHIEVEMENTS, ...ALL_SPECIAL_ACHIEVEMENTS]))}>[+]</span>}
						</h4>
						{ALL_TIERED_ACHIEVEMENTS.map((a) => {
							const hasTier = myAchievements.reduce((max, ma) => ma.key === a ? Math.max(max, ma.tier) : max, -1);
							const hasSeen1 = myAchievements.findIndex((ma) => ma.key === a && ma.tier === 1 && ma.hasSeen) !== -1;
							const hasSeen2 = myAchievements.findIndex((ma) => ma.key === a && ma.tier === 2 && ma.hasSeen) !== -1;
							const hasSeen3 = myAchievements.findIndex((ma) => ma.key === a && ma.tier === 3 && ma.hasSeen) !== -1;
							const isExpanded = expandedAchievements.has(a);
							return <div key={a} className="d-flex">
								<div className={"d-flex gap-2" + (isExpanded ? " flex-column py-2" : "")} style={{ cursor: "pointer" }} onClick={() => { toggleAchievementState(a) }}>
									<div className="d-flex">
										<AchivementBadge achievement={a} tier={1} hasAchievement={hasTier >= 1} isExpanded={isExpanded} showNotificationBadge={hasTier >= 1 && !hasSeen1} />
									</div>
									{hasTier >= 1 && <div className="d-flex">
										<AchivementBadge achievement={a} tier={2} hasAchievement={hasTier >= 2} isExpanded={isExpanded} showNotificationBadge={hasTier >= 2 && !hasSeen2} />
									</div>}
									{hasTier >= 2 && <div className="d-flex">
										<AchivementBadge achievement={a} tier={3} hasAchievement={hasTier >= 3} isExpanded={isExpanded} showNotificationBadge={hasTier >= 3 && !hasSeen3} />
									</div>}
								</div>
							</div>
						})}
						<div>Hidden Achievements</div>
						<div className="d-flex gap-2 flex-wrap">
							{ALL_SPECIAL_ACHIEVEMENTS.map((a) => {
								const hasAchievement = myAchievements.findIndex((ma) => ma.key === a) !== -1;
								const hasSeen = myAchievements.findIndex((ma) => ma.key === a && ma.hasSeen) !== -1;
								const isExpanded = expandedAchievements.has(a);
								return <div key={a} className={"d-flex" + (isExpanded ? " w-100" : "")}>
									<div className={"d-flex gap-2" + (isExpanded ? " flex-column py-2" : "")} style={{ cursor: "pointer" }} onClick={() => { toggleAchievementState(a) }}>
										<div className="d-flex">
											<AchivementBadge achievement={a} tier={0} hasAchievement={hasAchievement} isExpanded={isExpanded} showNotificationBadge={hasAchievement && !hasSeen} />
										</div>
									</div>
								</div>
							})}
						</div>
					</div>
					<div className="my-2 d-inline-flex flex-column gap-2">
						<h4>My Posts ({myPosts.length})</h4>
						{myPosts.length === 0 && <div>No posts yet.</div>}
						{myPosts.map((p) => (
							<a key={p.id} href={feedPostTitleToUrl(p.id, p.title)} className="d-flex align-items-center gap-2 text-body flex-wrap bg-body-tertiary p-2 rounded-1">
								<div className="fw-bold">
									{p.title}
								</div>
								{" · "}
								<div>{formatDate(p.date)}</div>
								{" · "}
								<div>Score: {p.score}</div>
							</a>
						))}
					</div>
					<div className="my-2 d-flex flex-column gap-2">
						<h4>My Comments ({myComments.length})</h4>
						{myComments.length === 0 && <div>No comments yet.</div>}
						{myComments.map((c) => (
							<div key={c.id} className="bg-body-tertiary p-2 rounded-1">
								<a href={feedPostTitleToUrl(c.postId, c.postTitle, c.id)} className="d-flex align-items-center gap-2 text-body flex-wrap">
									{c.postTitle ? (
										<div>
											On Post: <span className="fw-bold">{c.postTitle}</span>
										</div>
									) : (
										<div>
											[post removed]
										</div>
									)}

									{" · "}
									<div>{formatDate(c.date)}</div>
									{" · "}
									<div>Score: {c.score}</div>
								</a>
								<div className="bg-body-secondary ms-1 mt-1 p-2 rounded-1" style={{ maxHeight: "100px", overflow: "clip", pointerEvents: "none" }} >
									<FeedMarkdown>{c.content}</FeedMarkdown>
								</div>
							</div>
						))}
					</div>
				</div>
			)}
		</div>
	);
}

function AchievementToast() {
	const [achievements, setAchievements] = useState<Achievement[]>([]);

	useEffect(() => {
		const handler = (e: Event) => {
			setAchievements(prev => [...prev, ...(e as CustomEvent).detail]);
		};
		window.addEventListener("newAchievements", handler);
		return () => window.removeEventListener("newAchievements", handler);
	}, []);

	if (!achievements.length) { return null; }

	return (
		<div className="sticky-top" style={{ zIndex: 1030 }}>
			<div className="alert alert-success d-flex mt-2">
				<a href="/Feed/My" className="text-decoration-none text-success d-flex align-items-center gap-2 me-3 flex-wrap">
					<div>New achievement{achievements.length > 1 ? "s" : ""}! Click to view all.</div>
					{achievements.map((a) => {
						return <div key={a.id}>
							<AchivementBadge achievement={a.key} tier={a.tier} hasAchievement={true} isExpanded={false} showNotificationBadge={false} />
						</div>
					})}
				</a>
				<button className="btn-close ms-auto" onClick={() => { setAchievements([]); }} />
			</div>
		</div>
	);
}

function AdminQuery() {
	const [query, setQuery] = useState("");
	const [output, setOutput] = useState<string>("");
	const [expanded, setExpanded] = useState(false);

	async function sendQuery() {
		try {
			const res = await postForm("Query", new URLSearchParams({ query }));
			setOutput(JSON.stringify(res, null, 4));
		}
		catch (e) {
			setOutput(String(e instanceof Error ? e.message : e));
		}
	}

	return <div className="my-2">
		{expanded && <div className="d-flex flex-column gap-2">
			<TextareaAutosize
				className="form-control"
				value={query}
				onChange={(e) => setQuery(e.target.value)}
				autoFocus
			/>
			<button className="btn btn-primary" onClick={() => sendQuery()}>Query</button>
			<pre>{output}</pre>
		</div>}
		{!expanded && <button className="btn btn-danger" onClick={() => setExpanded(true)}>Admin</button>}
	</div>
}

const initStateJson = document.getElementById("feedappstate")?.dataset.state;
const initState = initStateJson ? JSON.parse(initStateJson) as Page : { type: "Home" } as Page;

export default function App() {
	const [page] = useState<Page>(initState);

	return (
		<div>
			<AchievementToast />
			{page.userName === "Masterjun" && <AdminQuery />}
			{page.type === "Home" && <HomePage />}
			{page.type === "Post" && <PostPage page={page} />}
			{page.type === "Comment" && <CommentPage page={page} />}
			{page.type === "My" && <MyPage />}
		</div>
	);
}

const app = document.getElementById("feedapp");
if (app) {
	const root = createRoot(app);
	root.render(<App />);
}