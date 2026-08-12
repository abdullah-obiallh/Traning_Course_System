/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { apiGet, apiPost } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";
function formatAvailableDate(date) {
    if (!date) return "";

    return new Date(date).toLocaleString("en-US", {
        day: "numeric",
        month: "long",
        year: "numeric",
        hour: "numeric",
        minute: "2-digit",
        hour12: true
    });
}
function StudentCourseLessonsPage() {
    const { enrollmentId } = useParams();
    const navigate = useNavigate();

    const [lessons, setLessons] = useState([]);
    const [progress, setProgress] = useState(null);
    const [course, setCourse] = useState(null);
    const [withdrawalReasons, setWithdrawalReasons] = useState([]);

    const [selectedReasonId, setSelectedReasonId] = useState("");
    const [withdrawalNote, setWithdrawalNote] = useState("");

    const [message, setMessage] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
    const [showWithdrawForm, setShowWithdrawForm] = useState(false);

    const user = getUser();

    useEffect(() => {
        if (!user) {
            navigate("/login");
            return;
        }

        if (user.userRole !== USER_ROLES.student) {
            setError("Only students can view this page.");
            setLoading(false);
            return;
        }

        loadPageData();
    }, [enrollmentId]);

    async function loadPageData() {
        try {
            const courseData = await apiGet(
                `/api/progress/enrollment/${enrollmentId}/course`
            );

            setCourse(courseData);
            const lessonsData = await apiGet(
                `/api/progress/enrollment/${enrollmentId}/lessons`
            );

            const progressData = await apiGet(
                `/api/progress/enrollment/${enrollmentId}`
            );

            const reasonsData = await apiGet("/api/withdrawalreasons");

            setLessons(lessonsData);
            setProgress(progressData);
            setWithdrawalReasons(reasonsData);
        } catch (err) {
            console.log(err.message)
            setError("Could not load course lessons.");
        } finally {
            setLoading(false);
        }
    }

    async function completeLesson(lessonId) {
        setMessage("");
        setError("");

        try {
            const result = await apiPost("/api/progress/complete-lesson", {
                enrollmentId: Number(enrollmentId),
                lessonId: lessonId
            });

            setProgress(result);
            setMessage("Lesson completed successfully.");

            await loadPageData();
        } catch (err) {
            setError(err.message);
        }
    }

    async function withdrawFromCourse(event) {
        event.preventDefault();

        setMessage("");
        setError("");

        if (!selectedReasonId) {
            setError("Please select a withdrawal reason.");
            return;
        }

        try {
            await apiPost("/api/enrollments/withdraw", {
                enrollmentId: Number(enrollmentId),
                withdrawalReasonId: Number(selectedReasonId),
                withdrawalNote: withdrawalNote
            });

            setMessage("You have withdrawn from this course.");
            await loadPageData();
        } catch (err) {
            setError(err.message);
        }
    }

    function canCompleteLessons() {
        return progress && progress.enrollmentStatus === "Enrolled";
    }

    if (loading) {
        return <div className="page-container">Loading lessons...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>{course?.title}</h1>

                    <p className="muted-text">
                        Course Lessons
                    </p>
                    <p className="muted-text">
                        Complete lessons to update your course progress.
                    </p>
                </div>

                <Link to="/student/my-courses" className="btn btn-outline">
                    Back to My Courses
                </Link>
            </section>

            {message && <div className="alert alert-success">{message}</div>}
            {error && <div className="alert alert-error">{error}</div>}

            {progress && (
                <section className="details-card">
                    <div className="details-header">
                        <div>
                            <span className="course-badge">
                                {progress.enrollmentStatus}
                            </span>
                            <h1>{progress.progressPercentage}% Completed</h1>
                            <p className="muted-text">
                                {progress.completedLessons} of {progress.totalLessons} lessons
                                completed.
                            </p>
                        </div>
                    </div>

                    <div className="progress-info large-progress">
                        <div className="progress-bar">
                            <div
                                className="progress-fill"
                                style={{ width: `${progress.progressPercentage}%` }}
                            ></div>
                        </div>
                    </div>
                </section>
            )}

            <section className="lessons-section">
                <h2>Lessons</h2>

                <div className="lessons-list">
                    {lessons.map((lesson) => (
                        <article className={`lesson-item ${lesson.isLocked ? "lesson-locked" : ""}`}
                            key={lesson.lessonId}>
                            <div className="lesson-number">{lesson.lessonOrder}</div>

                            <div className="lesson-content">
                                <div className="lesson-title-row">
                                    <div className="lesson-title-row">
                                        <h3>{lesson.title}</h3>

                                        {lesson.isLocked && (
                                            <span className="status status-warning">
                                                Locked
                                            </span>
                                        )}
                                    </div>

                                    {lesson.isCompleted ? (
                                        <span className="status status-completed">Completed</span>
                                    ) : (
                                        <span className="status status-active">Not completed</span>
                                    )}
                                </div>

                                {lesson.isLocked ? (
                                    <p className="muted-text">
                                        Available on: {formatAvailableDate(lesson.availableFrom)}
                                    </p>
                                ) : (
                                    <p>{lesson.content || "No content available."}</p>
                                )}

                                {!lesson.isLocked && lesson.videoUrl && (
                                    <a
                                        className="lesson-link"
                                        href={lesson.videoUrl}
                                        target="_blank"
                                        rel="noreferrer"
                                    >
                                        Open Link
                                    </a>
                                )}

                                {!lesson.isLocked &&
                                    !lesson.isCompleted &&
                                    canCompleteLessons() && (
                                    <button
                                        className="btn btn-primary"
                                        onClick={() => completeLesson(lesson.lessonId)}
                                    >
                                        Mark as Completed
                                    </button>
                                )}
                            </div>
                        </article>
                    ))}
                </div>
            </section>

            {progress && progress.enrollmentStatus === "Enrolled" && (
                <>
                    <section className="withdraw-toggle-section">

                        <button
                            className="btn btn-danger"
                            onClick={() => setShowWithdrawForm(!showWithdrawForm)}
                        >
                            {showWithdrawForm
                                ? "Hide Withdrawal Form"
                                : "Withdraw From Course"}
                        </button>

                    </section>
                    {showWithdrawForm && (

                 <section className="withdraw-card">
                    <h2>Withdraw from Course</h2>
                    <p className="muted-text">
                        Select a reason if you no longer want to continue this course.
                    </p>

                    <form onSubmit={withdrawFromCourse} className="form">
                        <div className="form-group">
                            <label>Withdrawal Reason</label>
                            <select
                                value={selectedReasonId}
                                onChange={(e) => setSelectedReasonId(e.target.value)}
                            >
                                <option value="">Select reason</option>

                                {withdrawalReasons.map((reason) => (
                                    <option
                                        key={reason.withdrawalReasonId}
                                        value={reason.withdrawalReasonId}
                                    >
                                        {reason.reasonText}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="form-group">
                            <label>Note</label>
                            <textarea
                                value={withdrawalNote}
                                onChange={(e) => setWithdrawalNote(e.target.value)}
                                placeholder="Optional note"
                            ></textarea>
                        </div>

                        <button className="btn btn-danger" type="submit">
                            Withdraw from Course
                        </button>
                    </form>
                        </section>)}
                </>
            )}
        </main>
    );
}

export default StudentCourseLessonsPage;