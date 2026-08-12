/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { apiDelete, apiGet, apiPost, apiPut } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";

function TeacherCourseLessonsPage() {
    const { courseId } = useParams();
    const navigate = useNavigate();

    const user = getUser();

    const [lessons, setLessons] = useState([]);
    const [course, setCourse] = useState(null);
    const [form, setForm] = useState({
        title: "",
        content: "",
        videoUrl: "",
        lessonOrder: "",
        availableFrom: ""
    });

    const [editingLessonId, setEditingLessonId] = useState(null);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!user) {
            navigate("/login");
            return;
        }

        if (user.userRole !== USER_ROLES.instructor) {
            setError("Only instructors can view this page.");
            setLoading(false);
            return;
        }

        loadLessons();
    }, [courseId]);

    async function loadLessons() {
        try {
            const courseData = await apiGet(
                `/api/teacher/courses/${courseId}?instructorId=${user.userId}`
            );

            setCourse(courseData);
            const data = await apiGet(
                `/api/teacher/courses/${courseId}/lessons?instructorId=${user.userId}`
            );

            setLessons(data);
        } catch (err) {
            console.log(err.message)
            setError("Could not load lessons.");
        } finally {
            setLoading(false);
        }
    }

    function handleChange(event) {
        const { name, value } = event.target;

        setForm({
            ...form,
            [name]: value
        });
    }

    function resetForm() {
        setForm({
            title: "",
            content: "",
            videoUrl: "",
            lessonOrder: "",
            availableFrom: ""
        });

        setEditingLessonId(null);
    }

    function startEdit(lesson) {
        setEditingLessonId(lesson.lessonId);

        setForm({
            title: lesson.title,
            content: lesson.content || "",
            videoUrl: lesson.videoUrl || "",
            lessonOrder: lesson.lessonOrder,
            availableFrom: lesson.availableFrom
                ? lesson.availableFrom.substring(0, 16)
                : ""
        });
    }

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setError("");

        if (!form.title.trim()) {
            setError("Lesson title is required.");
            return;
        }

        if (Number(form.lessonOrder) <= 0) {
            setError("Lesson order must be greater than zero.");
            return;
        }
        if (!form.availableFrom) {
            setError("Please select the lesson availability date.");
            return;
        }
        const lessonData = {
            instructorId: user.userId,
            title: form.title,
            content: form.content,
            videoUrl: form.videoUrl,
            lessonOrder: Number(form.lessonOrder),
            availableFrom: form.availableFrom
        };

        try {
            if (editingLessonId) {
                await apiPut(`/api/teacher/lessons/${editingLessonId}`, lessonData);
                setMessage("Lesson updated successfully.");
            } else {
                await apiPost(`/api/teacher/courses/${courseId}/lessons`, lessonData);
                setMessage("Lesson added successfully.");
            }

            resetForm();
            await loadLessons();
        } catch (err) {
            setError(err.message);
        }
    }

    async function deleteLesson(lessonId) {
        setMessage("");
        setError("");

        const confirmed = window.confirm("Are you sure you want to delete this lesson?");

        if (!confirmed) {
            return;
        }

        try {
            await apiDelete(`/api/teacher/lessons/${lessonId}?instructorId=${user.userId}`);
            setMessage("Lesson deleted successfully.");
            await loadLessons();
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) {
        return <div className="page-container">Loading lessons...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>
                        {course?.title}
                    </h1>

                    <p className="muted-text">
                        Manage Lessons
                    </p>
                    <p className="muted-text">
                        Add, update, and remove lessons for this course.
                    </p>
                </div>

                <Link to="/teacher/courses" className="btn btn-outline">
                    Back to Courses
                </Link>
            </section>

            {message && <div className="alert alert-success">{message}</div>}
            {error && <div className="alert alert-error">{error}</div>}

            <section className="management-layout">
                <div className="management-form-card">
                    <h2>{editingLessonId ? "Update Lesson" : "Add New Lesson"}</h2>

                    <form onSubmit={handleSubmit} className="form">
                        <div className="form-group">
                            <label>Lesson Title</label>
                            <input
                                name="title"
                                value={form.title}
                                onChange={handleChange}
                                placeholder="Lesson title"
                            />
                        </div>

                        <div className="form-group">
                            <label>Lesson Order</label>
                            <input
                                name="lessonOrder"
                                type="number"
                                value={form.lessonOrder}
                                onChange={handleChange}
                                placeholder="1"
                            />
                        </div>
                        <div className="form-group">
                            <label>Available From</label>

                            <input
                                type="datetime-local"
                                name="availableFrom"
                                value={form.availableFrom}
                                onChange={handleChange}
                                min={new Date().toISOString().slice(0, 16)}
                            />
                        </div>
                        <div className="form-group">
                            <label>URL</label>
                            <input
                                name="videoUrl"
                                value={form.videoUrl}
                                onChange={handleChange}
                                placeholder="Optional link"
                            />
                        </div>

                        <div className="form-group">
                            <label>Content</label>
                            <textarea
                                name="content"
                                value={form.content}
                                onChange={handleChange}
                                placeholder="Lesson content"
                            ></textarea>
                        </div>

                        <button className="btn btn-primary full-width" type="submit">
                            {editingLessonId ? "Update Lesson" : "Add Lesson"}
                        </button>

                        {editingLessonId && (
                            <button
                                className="btn btn-outline full-width top-space"
                                type="button"
                                onClick={resetForm}
                            >
                                Cancel Edit
                            </button>
                        )}
                    </form>
                </div>

                <div className="management-list-card">
                    <h2>Lessons List</h2>

                    {lessons.length === 0 && (
                        <p className="muted-text">No lessons added yet.</p>
                    )}

                    <div className="lessons-list">
                        {lessons.map((lesson) => (
                            <article className="lesson-item" key={lesson.lessonId}>
                                <div className="lesson-number">{lesson.lessonOrder}</div>

                                <div className="lesson-content">
                                    <h3>{lesson.title}</h3>
                                    <p>{lesson.content || "No content available."}</p>

                                    {lesson.videoUrl && (
                                        <a
                                            className="lesson-link"
                                            href={lesson.videoUrl}
                                            target="_blank"
                                            rel="noreferrer"
                                        >
                                            Open Link
                                        </a>
                                    )}

                                    <div className="row-actions">
                                        <button
                                            className="btn btn-outline"
                                            onClick={() => startEdit(lesson)}
                                        >
                                            Edit
                                        </button>

                                        <button
                                            className="btn btn-danger"
                                            onClick={() => deleteLesson(lesson.lessonId)}
                                        >
                                            Delete
                                        </button>
                                    </div>
                                </div>
                            </article>
                        ))}
                    </div>
                </div>
            </section>
        </main>
    );
}

export default TeacherCourseLessonsPage;