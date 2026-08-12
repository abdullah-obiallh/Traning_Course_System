/* eslint-disable react-hooks/set-state-in-effect */
/* eslint-disable react-hooks/immutability */
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiGet } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";
import { formatDate } from "../utils/formatters";
function AdminDashboardPage() {
    const navigate = useNavigate();
    const user = getUser();

    const [summary, setSummary] = useState(null);
    const [courseStats, setCourseStats] = useState([]);
    const [withdrawalStats, setWithdrawalStats] = useState([]);
    const [selectedWithdrawals, setSelectedWithdrawals] = useState(null);

    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!user) {
            navigate("/login");
            return;
        }

        if (user.userRole !== USER_ROLES.admin) {
            setError("Only admins can view this page.");
            setLoading(false);
            return;
        }

        loadDashboard();
    }, []);

    async function loadDashboard() {
        try {
            const summaryData = await apiGet("/api/dashboard/summary");
            const coursesData = await apiGet("/api/dashboard/courses-statistics");
            const withdrawalData = await apiGet("/api/dashboard/withdrawal-reasons");

            setSummary(summaryData);
            setCourseStats(coursesData);
            setWithdrawalStats(withdrawalData);
        } catch (err) {
            console.log(err.message)
            setError("Could not load dashboard data.");
        } finally {
            setLoading(false);
        }
    }

    async function loadCourseWithdrawals(courseId) {
        setError("");

        try {
            const data = await apiGet(`/api/dashboard/courses/${courseId}/withdrawals`);
            setSelectedWithdrawals(data);
        } catch (err) {
            console.log(err.message)
            setError("Could not load course withdrawals.");
        }
    }

    if (loading) {
        return <div className="page-container">Loading dashboard...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>Admin Dashboard</h1>
                    <p className="muted-text">
                        Monitor courses, students, enrollments, and withdrawals.
                    </p>
                </div>

                <Link to="/admin/courses" className="btn btn-primary">
                    Manage Courses
                </Link>
            </section>

            {error && <div className="alert alert-error">{error}</div>}

            {summary && (
                <section className="stats-grid">
                    <div className="stat-card">
                        <span>Courses</span>
                        <strong>{summary.coursesCount}</strong>
                    </div>

                    <div className="stat-card">
                        <span>Published Courses</span>
                        <strong>{summary.publishedCoursesCount}</strong>
                    </div>

                    <div className="stat-card">
                        <span>Students</span>
                        <strong>{summary.studentsCount}</strong>
                    </div>

                    <div className="stat-card">
                        <span>Instructors</span>
                        <strong>{summary.instructorsCount}</strong>
                    </div>

                    <div className="stat-card">
                        <span>Total Enrollments</span>
                        <strong>{summary.totalEnrollmentsCount}</strong>
                    </div>

                    <div className="stat-card">
                        <span>Active</span>
                        <strong>{summary.activeEnrollmentsCount}</strong>
                    </div>

                    <div className="stat-card">
                        <span>Completed</span>
                        <strong>{summary.completedEnrollmentsCount}</strong>
                    </div>

                    <div className="stat-card">
                        <span>Withdrawn</span>
                        <strong>{summary.withdrawnEnrollmentsCount}</strong>
                    </div>
                </section>
            )}

            <section className="dashboard-section">
                <h2>Courses Statistics</h2>

                <div className="table-card">
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Course</th>
                                <th>Instructor</th>
                                <th>Total</th>
                                <th>Active</th>
                                <th>Completed</th>
                                <th>Withdrawn</th>
                                <th>Withdrawal %</th>
                                <th>Completion</th>
                                <th>Details</th>
                            </tr>
                        </thead>

                        <tbody>
                            {courseStats.map((course) => (
                                <tr key={course.courseId}>
                                    <td>
                                        <strong>{course.courseTitle}</strong>
                                    </td>

                                    <td>{course.instructorName}</td>
                                    <td>{course.totalEnrollments}</td>
                                    <td>{course.activeStudents}</td>
                                    <td>{course.completedStudents}</td>
                                    <td>{course.withdrawnStudents}</td>

                                    <td>
                                        <strong>{course.withdrawalPercentage}%</strong>
                                    </td>

                                    <td>
                                        <div className="table-progress">
                                            <div className="progress-bar">
                                                <div
                                                    className="progress-fill"
                                                    style={{ width: `${course.completionPercentage}%` }}
                                                ></div>
                                            </div>

                                            <strong>{course.completionPercentage}%</strong>
                                        </div>
                                    </td>

                                    <td>
                                        <button
                                            className="btn btn-outline"
                                            onClick={() => loadCourseWithdrawals(course.courseId)}
                                        >
                                            Withdrawals
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </section>

            {selectedWithdrawals && (
                <section className="dashboard-section">
                    <div className="withdrawals-details-header">
                        <div>
                            <h2>Withdrawals Details</h2>
                            <p className="muted-text">
                                Course: {selectedWithdrawals.courseTitle}
                            </p>
                        </div>

                        <button
                            className="btn btn-outline"
                            onClick={() => setSelectedWithdrawals(null)}
                        >
                            Close
                        </button>
                    </div>

                    <section className="stats-grid small-stats">
                        <div className="stat-card">
                            <span>Total Enrollments</span>
                            <strong>{selectedWithdrawals.totalEnrollments}</strong>
                        </div>

                        <div className="stat-card">
                            <span>Withdrawn Students</span>
                            <strong>{selectedWithdrawals.withdrawnCount}</strong>
                        </div>

                        <div className="stat-card">
                            <span>Withdrawal Percentage</span>
                            <strong>{selectedWithdrawals.withdrawalPercentage}%</strong>
                        </div>
                    </section>

                    {selectedWithdrawals.withdrawals.length === 0 ? (
                        <div className="empty-box">
                            <h2>No withdrawals</h2>
                            <p>No student has withdrawn from this course.</p>
                        </div>
                    ) : (
                        <div className="table-card">
                            <table className="data-table">
                                <thead>
                                    <tr>
                                        <th>Student</th>
                                        <th>Reason</th>
                                        <th>Note</th>
                                        <th>Withdrawn At</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {selectedWithdrawals.withdrawals.map((item) => (
                                        <tr key={item.enrollmentId}>
                                            <td>
                                                <strong>{item.studentName}</strong>
                                                <span>{item.studentEmail}</span>
                                            </td>

                                            <td>{item.reasonText}</td>

                                            <td>
                                                {item.withdrawalNote && item.withdrawalNote.trim()
                                                    ? item.withdrawalNote
                                                    : "-"}
                                            </td>

                                            <td>
                                                {formatDate(item.withdrawnAt)}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </section>
            )}

            <section className="dashboard-section">
                <h2>Withdrawal Reasons - Overall</h2>

                <div className="withdrawal-grid">
                    {withdrawalStats.map((reason) => (
                        <div className="withdrawal-stat-card" key={reason.withdrawalReasonId}>
                            <span>{reason.reasonText}</span>
                            <strong>{reason.withdrawalsCount}</strong>
                        </div>
                    ))}
                </div>
            </section>
        </main>
    );
}

export default AdminDashboardPage;