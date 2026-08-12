/* eslint-disable react-hooks/set-state-in-effect */
/* eslint-disable react-hooks/immutability */
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiGet } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";
import { formatDate } from "../utils/formatters";

function StudentMyCoursesPage() {
    const navigate = useNavigate();

    const [courses, setCourses] = useState([]);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

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

        loadMyCourses();
    }, []);

    async function loadMyCourses() {
        try {
            const data = await apiGet(`/api/enrollments/student/${user.userId}`);
            setCourses(data);
        } catch (err) {
            console.log(err.message)
            setError("Could not load your courses.");
        } finally {
            setLoading(false);
        }
    }

    function getStatusClass(status) {
        if (status === "Completed") return "status status-completed";
        if (status === "Withdrawn") return "status status-withdrawn";
        return "status status-active";
    }

    if (loading) {
        return <div className="page-container">Loading your courses...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>My Courses</h1>
                    <p className="muted-text">
                        Track your enrolled courses and learning progress.
                    </p>
                </div>

                <Link to="/courses" className="btn btn-primary">
                    Browse Courses
                </Link>
            </section>

            {error && <div className="alert alert-error">{error}</div>}

            {courses.length === 0 && !error && (
                <div className="empty-box">
                    <h2>No courses yet</h2>
                    <p>You have not enrolled in any course.</p>
                    <Link to="/courses" className="btn btn-primary">
                        View Available Courses
                    </Link>
                </div>
            )}

            <section className="cards-grid">
                {courses.map((course) => (
                    <article className="course-card" key={course.enrollmentId}>
                        <div className={getStatusClass(course.status)}>
                            {course.status}
                        </div>

                        <h2>{course.courseTitle}</h2>

                        <div className="progress-info">
                            <div className="progress-header">
                                <span>Progress</span>
                                <strong>{course.progressPercentage}%</strong>
                            </div>

                            <div className="progress-bar">
                                <div
                                    className="progress-fill"
                                    style={{ width: `${course.progressPercentage}%` }}
                                ></div>
                            </div>
                        </div>

                        <div className="course-meta">
                            <span>{course.completedLessons} completed</span>
                            <span>{course.totalLessons} lessons</span>
                        </div>

                        <p className="instructor-name">
                            Enrolled at: {formatDate(course.enrolledAt)}
                        </p>

                        <Link
                            to={`/student/courses/${course.enrollmentId}/lessons`}
                            className="btn btn-primary full-width"
                        >
                            Open Course
                        </Link>
                    </article>
                ))}
            </section>
        </main>
    );
}

export default StudentMyCoursesPage;