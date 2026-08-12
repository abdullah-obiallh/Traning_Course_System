import { USER_ROLES } from "../constants/userRoles";

export function getHomePathForRole(role) {
    if (role === USER_ROLES.admin) {
        return "/admin/dashboard";
    }

    if (role === USER_ROLES.instructor) {
        return "/teacher/courses";
    }

    return "/student/my-courses";
}