import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";

import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

import CoursesPage from "./pages/CoursesPage";
import CourseDetailsPage from "./pages/CourseDetailsPage";

import StudentMyCoursesPage from "./pages/StudentMyCoursesPage";
import StudentCourseLessonsPage from "./pages/StudentCourseLessonsPage";

import TeacherCoursesPage from "./pages/TeacherCoursesPage";
import TeacherCourseLessonsPage from "./pages/TeacherCourseLessonsPage";
import TeacherCourseStudentsPage from "./pages/TeacherCourseStudentsPage";

import AdminDashboardPage from "./pages/AdminDashboardPage";
import AdminCoursesPage from "./pages/AdminCoursesPage";

import UnauthorizedPage from "./pages/UnauthorizedPage";
import NotFoundPage from "./pages/NotFoundPage";
import { USER_ROLES } from "./constants/userRoles";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import AdminUsersPage from "./pages/AdminUsersPage";

function App() {
    return (
        <BrowserRouter>
            <Navbar />

            <Routes>
                <Route path="/" element={<Navigate to="/courses" />} />


                <Route path="/courses" element={<CoursesPage />} />
                <Route path="/courses/:id" element={<CourseDetailsPage />} />


                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/forgot-password" element={<ForgotPasswordPage />} />

                <Route
                    path="/student/my-courses"
                    element={
                        <ProtectedRoute allowedRoles={[USER_ROLES.student]}>
                            <StudentMyCoursesPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/student/courses/:enrollmentId/lessons"
                    element={
                        <ProtectedRoute allowedRoles={[USER_ROLES.student]}>
                            <StudentCourseLessonsPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/teacher/courses"
                    element={
                        <ProtectedRoute allowedRoles={[USER_ROLES.instructor]}>
                            <TeacherCoursesPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/teacher/courses/:courseId/lessons"
                    element={
                        <ProtectedRoute allowedRoles={[USER_ROLES.instructor]}>
                            <TeacherCourseLessonsPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/teacher/courses/:courseId/students"
                    element={
                        <ProtectedRoute allowedRoles={[USER_ROLES.instructor]}>
                            <TeacherCourseStudentsPage />
                        </ProtectedRoute>
                    }
                />
                <Route
                    path="/admin/users"
                    element={
                        <ProtectedRoute allowedRoles={["Admin"]}>
                            <AdminUsersPage />
                        </ProtectedRoute>
                    }
                />
                <Route
                    path="/admin/dashboard"
                    element={
                        <ProtectedRoute allowedRoles={[USER_ROLES.admin]}>
                            <AdminDashboardPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/admin/courses"
                    element={
                        <ProtectedRoute allowedRoles={[USER_ROLES.admin]}>
                            <AdminCoursesPage />
                        </ProtectedRoute>
                    }
                />

                <Route path="/unauthorized" element={<UnauthorizedPage />} />

                <Route path="*" element={<NotFoundPage />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;