import type {UserResponseDto} from "@/api.ts";
import { api } from "@/apiClient";
import {useEffect, useState} from "react";
import toast from "react-hot-toast";
import {EditUserView} from "./EditUserView.tsx";
import {ChangePasswordView} from "@/views/ChangePasswordView.tsx";

interface UserManagementViewProps {
    currentUser: UserResponseDto;
    onLogout: () => void;
}

export function UserManagementView(props: UserManagementViewProps) {
    const [users, setUsers] = useState<UserResponseDto[]>([]);
    const [loading, setLoading] = useState(true);
    
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingUser, setEditingUser] = useState<UserResponseDto | null>(null);

    const [passwordTargetUser, setPasswordTargetUser] = useState<UserResponseDto | null>(null);
    
    const getUsers = async () => {
        setLoading(true);
        try{
            const response = await api.users.usersGetUsers();
            setUsers(response.data);
        }catch (error:any){
            const message =
                error?.error?.detail ||
                error?.error?.title ||
                error?.message ||
                "Failed to load users.";
            toast.error(message);
        } finally{
            setLoading(false);
        }
    };
    useEffect(() => {
        getUsers();
    }, []);
    
    const deleteUser = async (id: string) => {
        if(!confirm("Are you sure you want to delete this user?"))return;
        
        try {
            await api.users.usersDeleteUser({id: id});
            toast.success("User deleted successfully.");
            getUsers();
        } catch (error: any) {
            const message =
                error?.error?.detail ||
                error?.error?.title ||
                error?.message ||
                "Failed to delete user.";
            toast.error(message);
        }
    };

    const handleCreateEdit = async (formData: {
        firstName: string;
        lastName: string;
        login: string;
        role: string;
        password?: string;
    }) => {
        try {
            if (editingUser) {
                await api.users.usersUpdateUser(
                    {
                        firstName: formData.firstName,
                        lastName: formData.lastName,
                        role: formData.role,
                    },
                    {
                        userId: editingUser.userId!
                    }
                );
                toast.success("User updated successfully.");
            } else {
                await api.users.usersCreateUser({
                    firstName: formData.firstName,
                    lastName: formData.lastName,
                    role: formData.role,
                    password: formData.password!,
                });
                toast.success("User created successfully.");
            }

            setIsModalOpen(false);
            setEditingUser(null);
            getUsers();
        } catch (error: any) {
            const message =
                error?.error?.detail ||
                error?.error?.title ||
                error?.message ||
                "Operation failed.";
            toast.error(message);
        }
    };

    const canChangePassword = (targetUser: UserResponseDto) => {
        return props.currentUser.role === "Admin" || targetUser.userId === props.currentUser.userId;
    };

    const handleChangePasswordSubmit = async (newPassword: string) => {
        if (!passwordTargetUser?.userId) return;

        try {
            // Generated client expects: (data: ChangePasswordRequestDto, query: { userId: string })
            await api.users.usersChangePassword(
                {
                    newPassword: newPassword,
                },
                {
                    userId: passwordTargetUser.userId,
                }
            );
            toast.success(`Password updated for ${passwordTargetUser.login}.`);
            setPasswordTargetUser(null);
        } catch (error: any) {
            const message =
                error?.error?.detail ||
                error?.error?.title ||
                error?.message ||
                "Failed to change password.";
            toast.error(message);
        }
    };

    const canCreate = props.currentUser.role === "Admin" || props.currentUser.role === "Superuser";

    const canModifyRow = (targetUser: UserResponseDto) => {
        if (props.currentUser.role === "Admin") return true;
        if (props.currentUser.role === "Superuser") return targetUser.role === "User";
        return false;
    };


    return (
        <div className="container">
            <div className="header-row">
                <h2>User Management</h2>
                <div>
          <span>
            Signed in as: <strong>{props.currentUser.login}</strong> ({props.currentUser.role})
          </span>
                    <button
                        type="button"
                        onClick={() => setPasswordTargetUser(props.currentUser)}
                    >
                        Change My Password
                    </button>
                    <button type="button" onClick={props.onLogout}>
                        Sign Out
                    </button>
                </div>
            </div>

            <hr />

            {canCreate && (
                <div className="actions-bar">
                    <button
                        type="button"
                        className="btn-primary"
                        onClick={() => {
                            setEditingUser(null);
                            setIsModalOpen(true);
                        }}
                    >
                        + Create New User
                    </button>
                </div>
            )}

            {loading ? (
                <p>Loading users...</p>
            ) : (
                <table className="table">
                    <thead>
                    <tr>
                        <th>First Name</th>
                        <th>Last Name</th>
                        <th>Login</th>
                        <th>Role</th>
                        {(props.currentUser.role === "Admin" || props.currentUser.role === "Superuser") && (
                            <th>Actions</th>
                        )}
                    </tr>
                    </thead>
                    <tbody>
                    {users.length === 0 ? (
                        <tr>
                            <td colSpan={5}>No users found.</td>
                        </tr>
                    ) : (
                        users.map((u) => (
                            <tr key={u.userId}>
                                <td>{u.firstName}</td>
                                <td>{u.lastName}</td>
                                <td>{u.login}</td>
                                <td>{u.role}</td>
                                {(props.currentUser.role === "Admin" || props.currentUser.role === "Superuser") && (
                                    <td>
                                        {canModifyRow(u) && (
                                            < div className="button-row">
                                                <button
                                                    type="button"
                                                    onClick={() => {
                                                        setEditingUser(u);
                                                        setIsModalOpen(true);
                                                    }}
                                                >
                                                    Edit
                                                </button>

                                                {canChangePassword(u) && (
                                                    <button
                                                        type="button"
                                                        onClick={() => setPasswordTargetUser(u)}
                                                    >
                                                        Password
                                                    </button>
                                                )}
                                                
                                                
                                                {u.userId !== props.currentUser.userId && (
                                                    <button
                                                        type="button"
                                                        className="btn-danger"
                                                        onClick={() => deleteUser(u.userId!)}
                                                    >
                                                        Delete
                                                    </button>
                                                )}
                                            </div>
                                        )}
                                    </td>
                                )}
                            </tr>
                        ))
                    )}
                    </tbody>
                </table>
            )}
            {isModalOpen && (
            <EditUserView
                currentUserRole={props.currentUser.role!}
                initialUser={editingUser}
                onClose={() => {
                    setIsModalOpen(false);
                    setEditingUser(null);
                }}
                onSubmit={handleCreateEdit}
            />
            )}

            {passwordTargetUser && (
                <ChangePasswordView
                    targetUserLogin={passwordTargetUser.login!}
                    onClose={() => setPasswordTargetUser(null)}
                    onSubmit={handleChangePasswordSubmit}
                />
            )}
            
        </div>
    );
    
    
}