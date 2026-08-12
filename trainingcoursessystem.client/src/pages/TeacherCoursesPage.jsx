/* eslint-disable react-hooks/immutability */
/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiGet } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";

function TeacherCoursesPage() {
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

        if (user.userRole !== USER_ROLES.instructor) {
            setError("Only instructors can view this page.");
            setLoading(false);
            return;
        }

        loadCourses();
    }, []);

    async function loadCourses() {
        try {
            const data = await apiGet(`/api/teacher/courses?instructorId=${user.userId}`);
            setCourses(data);
        } catch (err) {
            console.log(err.message)
            setError("Could not load teacher courses.");
        } finally {
            setLoading(false);
        }
    }

    if (loading) {
        return <div className="page-container">Loading teacher courses...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>Teacher Panel</h1>
                    <p className="muted-text">
                        Manage your assigned courses, lessons, and student progress.
                    </p>
                </div>
            </section>

            {error && <div className="alert alert-error">{error}</div>}

            {courses.length === 0 && !error && (
                <div className="empty-box">
                    <h2>No assigned courses</h2>
                    <p>You do not have any assigned courses yet.</p>
                </div>
            )}

            <section className="cards-grid">
                {courses.map((course) => (
                    <article className="course-card" key={course.courseId}>
                        <div className={course.isPublished ? "status status-active" : "status status-withdrawn"}>
                            {course.isPublished ? "Published" : "Not Published"}
                        </div>

                        <h2>{course.title}</h2>

                        <p className="course-description">
                            {course.category || "No category"} - {course.levelName || "No level"}
                        </p>

                        <div className="course-meta">
                            <span>{course.durationHours} hours</span>
                            <span>{course.lessonsCount} lessons</span>
                            <span>{course.studentsCount} students</span>
                        </div>

                        <div className="card-actions">
                            <Link
                                to={`/teacher/courses/${course.courseId}/lessons`}
                                className="btn btn-primary"
                            >
                                Manage Lessons
                            </Link>

                            <Link
                                to={`/teacher/courses/${course.courseId}/students`}
                                className="btn btn-outline"
                            >
                                View Students
                            </Link>
                        </div>
                    </article>
                ))}
            </section>
        </main>
    );
}

export default TeacherCoursesPage;