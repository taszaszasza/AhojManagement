import { useState, type FormEvent } from "react";
import type { UserResponseDto } from "@/api.ts";

interface EditUserViewProps {
    currentUserRole: string;
    initialUser: UserResponseDto | null;
    onClose: () => void;
    onSubmit: (formData: {
        firstName: string;
        lastName: string;
        login: string;
        role: string;
        password?: string;
    }) => void;
}

export function EditUserView({
                              currentUserRole,
                              initialUser,
                              onClose,
                              onSubmit,
                          }: EditUserViewProps) {
    const isEditing = Boolean(initialUser);

    const [firstName, setFirstName] = useState(initialUser?.firstName || "");
    const [lastName, setLastName] = useState(initialUser?.lastName || "");
    const [login, setLogin] = useState(initialUser?.login || "");
    const [role, setRole] = useState(initialUser?.role || "User");
    const [password, setPassword] = useState("");

    const handleSubmit = (e: FormEvent) => {
        e.preventDefault();
        onSubmit({
            firstName,
            lastName,
            login,
            role,
            ...(isEditing ? {} : { password }),
        });
    };

    return (
        <div className="modal-backdrop">
            <div className="modal-content">
                <h3>{isEditing ? `Edit User: ${initialUser?.login}` : "Create New User"}</h3>

                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>First Name: </label>
                        <input
                            type="text"
                            value={firstName}
                            onChange={(e) => setFirstName(e.target.value)}
                            required
                        />
                    </div>

                    <div className="form-group">
                        <label>Last Name: </label>
                        <input
                            type="text"
                            value={lastName}
                            onChange={(e) => setLastName(e.target.value)}
                            required
                        />
                    </div>
                    

                    {!isEditing && (
                        <div className="form-group">
                            <label>Password: </label>
                            <input
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                            />
                        </div>
                    )}

                    <div className="form-group">
                        <label>Role: </label>
                        <select
                            value={role}
                            onChange={(e) => setRole(e.target.value)}
                            disabled={currentUserRole === "Superuser"}
                        >
                            <option value="User">User</option>
                            {currentUserRole === "Admin" && (
                                <>
                                    <option value="Superuser">Superuser</option>
                                    <option value="Admin">Admin</option>
                                </>
                            )}
                        </select>
                    </div>

                    <hr />
                    
                    <div className="modal-actions">

                    <button type="submit">
                        {isEditing ? "Save Changes" : "Create"}
                    </button>
                    <button type="button" onClick={onClose} style={{ marginLeft: 8 }}>
                        Cancel
                    </button>
                    </div>
                </form>
            </div>
        </div>
    );
}