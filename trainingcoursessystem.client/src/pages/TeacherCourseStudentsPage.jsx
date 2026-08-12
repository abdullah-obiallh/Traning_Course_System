/* eslint-disable react-hooks/immutability */
/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { apiGet } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";

function TeacherCourseStudentsPage() {
    const { courseId } = useParams();
    const navigate = useNavigate();

    const user = getUser();

    const [students, setStudents] = useState([]);
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

        loadStudents();
    }, [courseId]);

    async function loadStudents() {
        try {
            const data = await apiGet(
                `/api/teacher/courses/${courseId}/students?instructorId=${user.userId}`
            );

            setStudents(data);
        } catch (err) {
            console.log(err.message)
            setError("Could not load course students.");
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
        return <div className="page-container">Loading students...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>Course Students</h1>
                    <p className="muted-text">
                        View enrolled students and track their progress.
                    </p>
                </div>

                <Link to="/teacher/courses" className="btn btn-outline">
                    Back to Courses
                </Link>
            </section>

            {error && <div className="alert alert-error">{error}</div>}

            {students.length === 0 && !error && (
                <div className="empty-box">
                    <h2>No students yet</h2>
                    <p>No students are currently enrolled in this course.</p>
                </div>
            )}

            <section className="table-card">
                <table className="data-table">
                    <thead>
                        <tr>
                            <th>Student</th>
                            <th>Status</th>
                            <th>Progress</th>
                            <th>Lessons</th>
                            <th>Withdrawal Reason</th>
                        </tr>
                    </thead>

                    <tbody>
                        {students.map((student) => (
                            <tr key={student.enrollmentId}>
                                <td>
                                    <strong>{student.studentName}</strong>
                                    <span>{student.studentEmail}</span>
                                </td>

                                <td>
                                    <span className={getStatusClass(student.status)}>
                                        {student.status}
                                    </span>
                                </td>

                                <td>
                                    <div className="table-progress">
                                        <div className="progress-bar">
                                            <div
                                                className="progress-fill"
                                                style={{ width: `${student.progressPercentage}%` }}
                                            ></div>
                                        </div>

                                        <strong>{student.progressPercentage}%</strong>
                                    </div>
                                </td>

                                <td>
                                    {student.completedLessons} / {student.totalLessons}
                                </td>

                                <td>
                                    {student.withdrawalReasonText || "-"}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </section>
        </main>
    );
}

export default TeacherCourseStudentsPage;