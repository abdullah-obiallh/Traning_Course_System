/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { USER_ROLES } from "../constants/userRoles";
import { Link, useNavigate } from "react-router-dom";
import { apiDelete, apiGet, apiPost, apiPut } from "../api/apiClient";
import { getUser } from "../auth/authStorage";

function AdminCoursesPage() {
    const navigate = useNavigate();
    const user = getUser();

    const [courses, setCourses] = useState([]);
    const [instructors, setInstructors] = useState([]);

    const [form, setForm] = useState({
        title: "",
        description: "",
        category: "",
        levelName: "",
        durationHours: "",
        instructorId: "",
        isPublished: true
    });

    const [editingCourseId, setEditingCourseId] = useState(null);

    const [message, setMessage] = useState("");
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

        loadData();
    }, []);

    async function loadData() {
        try {
            const coursesData = await apiGet("/api/admin/courses");
            const instructorsData = await apiGet("/api/admin/courses/instructors");

            setCourses(coursesData);
            setInstructors(instructorsData);
        } catch (err) {
            console.log(err.message)
            setError("Could not load courses data.");
        } finally {
            setLoading(false);
        }
    }

    function handleChange(event) {
        const { name, value, type, checked } = event.target;

        setForm({
            ...form,
            [name]: type === "checkbox" ? checked : value
        });
    }

    function resetForm() {
        setForm({
            title: "",
            description: "",
            category: "",
            levelName: "",
            durationHours: "",
            instructorId: "",
            isPublished: true
        });

        setEditingCourseId(null);
    }

    function startEdit(course) {
        setEditingCourseId(course.courseId);

        setForm({
            title: course.title,
            description: course.description || "",
            category: course.category || "",
            levelName: course.levelName || "",
            durationHours: course.durationHours,
            instructorId: course.instructorId,
            isPublished: course.isPublished
        });
    }

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setError("");

        if (!form.title.trim()) {
            setError("Course title is required.");
            return;
        }

        if (Number(form.durationHours) <= 0) {
            setError("Duration hours must be greater than zero.");
            return;
        }

        if (!form.instructorId) {
            setError("Please select an instructor.");
            return;
        }

        const courseData = {
            title: form.title,
            description: form.description,
            category: form.category,
            levelName: form.levelName,
            durationHours: Number(form.durationHours),
            instructorId: Number(form.instructorId),
            isPublished: form.isPublished
        };

        try {
            if (editingCourseId) {
                await apiPut(`/api/admin/courses/${editingCourseId}`, courseData);
                setMessage("Course updated successfully.");
            } else {
                await apiPost("/api/admin/courses", courseData);
                setMessage("Course added successfully.");
            }

            resetForm();
            await loadData();
        } catch (err) {
            setError(err.message);
        }
    }

    async function deleteCourse(courseId) {
        setMessage("");
        setError("");

        const confirmed = window.confirm("Are you sure you want to delete this course?");

        if (!confirmed) {
            return;
        }

        try {
            await apiDelete(`/api/admin/courses/${courseId}`);
            setMessage("Course deleted successfully.");
            await loadData();
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) {
        return <div className="page-container">Loading admin courses...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>Manage Courses</h1>
                    <p className="muted-text">
                        Add, update, publish, and delete training courses.
                    </p>
                </div>

                <Link to="/admin/dashboard" className="btn btn-outline">
                    Back to Dashboard
                </Link>
            </section>

            {message && <div className="alert alert-success">{message}</div>}
            {error && <div className="alert alert-error">{error}</div>}

            <section className="management-layout">
                <div className="management-form-card">
                    <h2>{editingCourseId ? "Update Course" : "Add New Course"}</h2>

                    <form onSubmit={handleSubmit} className="form">
                        <div className="form-group">
                            <label>Course Title</label>
                            <input
                                name="title"
                                value={form.title}
                                onChange={handleChange}
                                placeholder="Course title"
                            />
                        </div>

                        <div className="form-group">
                            <label>Description</label>
                            <textarea
                                name="description"
                                value={form.description}
                                onChange={handleChange}
                                placeholder="Course description"
                            ></textarea>
                        </div>

                        <div className="form-group">
                            <label>Category</label>
                            <input
                                name="category"
                                value={form.category}
                                onChange={handleChange}
                                placeholder="Programming"
                            />
                        </div>

                        <div className="form-group">
                            <label>Level</label>
                            <input
                                name="levelName"
                                value={form.levelName}
                                onChange={handleChange}
                                placeholder="Beginner"
                            />
                        </div>

                        <div className="form-group">
                            <label>Duration Hours</label>
                            <input
                                name="durationHours"
                                type="number"
                                value={form.durationHours}
                                onChange={handleChange}
                                placeholder="10"
                            />
                        </div>

                        <div className="form-group">
                            <label>Instructor</label>
                            <select
                                name="instructorId"
                                value={form.instructorId}
                                onChange={handleChange}
                            >
                                <option value="">Select instructor</option>

                                {instructors.map((instructor) => (
                                    <option
                                        key={instructor.instructorId}
                                        value={instructor.instructorId}
                                    >
                                        {instructor.fullName}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <label className="checkbox-row">
                            <input
                                name="isPublished"
                                type="checkbox"
                                checked={form.isPublished}
                                onChange={handleChange}
                            />
                            Published
                        </label>

                        <button className="btn btn-primary full-width" type="submit">
                            {editingCourseId ? "Update Course" : "Add Course"}
                        </button>

                        {editingCourseId && (
                            <button
                                type="button"
                                className="btn btn-outline full-width top-space"
                                onClick={resetForm}
                            >
                                Cancel Edit
                            </button>
                        )}
                    </form>
                </div>

                <div className="management-list-card">
                    <h2>Courses List</h2>

                    {courses.length === 0 && (
                        <p className="muted-text">No courses available.</p>
                    )}

                    <div className="admin-course-list">
                        {courses.map((course) => (
                            <article className="admin-course-item" key={course.courseId}>
                                <div>
                                    <div className={course.isPublished ? "status status-active" : "status status-withdrawn"}>
                                        {course.isPublished ? "Published" : "Not Published"}
                                    </div>

                                    <h3>{course.title}</h3>

                                    <p className="muted-text">
                                        {course.category || "No category"} - {course.levelName || "No level"}
                                    </p>

                                    <div className="course-meta">
                                        <span>{course.durationHours} hours</span>
                                        <span>{course.lessonsCount} lessons</span>
                                        <span>{course.enrollmentsCount} enrollments</span>
                                    </div>

                                    <p className="instructor-name">
                                        Instructor: {course.instructorName}
                                    </p>
                                </div>

                                <div className="row-actions">
                                    <button className="btn btn-outline" onClick={() => startEdit(course)}>
                                        Edit
                                    </button>

                                    <button className="btn btn-danger" onClick={() => deleteCourse(course.courseId)}>
                                        Delete
                                    </button>
                                </div>
                            </article>
                        ))}
                    </div>
                </div>
            </section>
        </main>
    );
}

export default AdminCoursesPage;