import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import API, { API_BASE } from "./api";
import "./index.css";

function App() {
    const [tasks, setTasks] = useState([]);
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [searchId, setSearchId] = useState("");
    const [foundTask, setFoundTask] = useState(null);
    const [editingTaskId, setEditingTaskId] = useState(null);
    const [editTitle, setEditTitle] = useState("");
    const [editDescription, setEditDescription] = useState("");
    const [editIsCompleted, setEditIsCompleted] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");
    const [successMessage, setSuccessMessage] = useState("");
    const [mailMessages, setMailMessages] = useState([]);
    const [mailSource, setMailSource] = useState("");
    const [socketStatus, setSocketStatus] = useState("Подключение...");

    const loadTasks = async () => {
        try {
            const response = await API.get("/tasks");
            setTasks(response.data);
            setErrorMessage("");
        } catch (error) {
            console.error("Ошибка загрузки задач:", error);
            setErrorMessage("Не удалось загрузить задачи.");
        }
    };
    useEffect(() => {
        let isMounted = true;

        const initialize = async () => {
            if (isMounted) {
                await loadTasks();
            }
        };

        initialize();

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE}/taskHub`)
            .withAutomaticReconnect()
            .build();

        connection.on("TasksChanged", async () => {
            if (isMounted) {
                await loadTasks();
            }
        });

        connection.onreconnecting(() => {
            setSocketStatus("Переподключение...");
        });

        connection.onreconnected(() => {
            setSocketStatus("Подключено");
            if (isMounted) {
                loadTasks();
            }
        });

        connection.onclose(() => {
            setSocketStatus("Отключено");
        });

        connection
            .start()
            .then(() => {
                setSocketStatus("Подключено");
            })
            .catch((error) => {
                console.error("Ошибка подключения SignalR:", error);
                setSocketStatus("Ошибка подключения");
            });

        return () => {
            isMounted = false;
            connection.stop();
        };
    }, []);

    const clearMessages = () => {
        setErrorMessage("");
        setSuccessMessage("");
    };

    const handleAddTask = async () => {
        if (!title.trim()) {
            setErrorMessage("Введите название задачи.");
            return;
        }

        try {
            clearMessages();

            await API.post("/tasks", {
                title,
                description
            });

            setTitle("");
            setDescription("");
            setSuccessMessage("Задача успешно добавлена.");
        } catch (error) {
            console.error("Ошибка добавления задачи:", error);
            setErrorMessage("Не удалось добавить задачу.");
        }
    };

    const handleDeleteTask = async (id) => {
        try {
            clearMessages();

            await API.delete(`/tasks/${id}`);

            if (foundTask && foundTask.id === id) {
                setFoundTask(null);
            }

            setSuccessMessage("Задача удалена.");
        } catch (error) {
            console.error("Ошибка удаления задачи:", error);
            setErrorMessage("Не удалось удалить задачу.");
        }
    };

    const handleFindTaskById = async () => {
        if (!searchId.trim()) {
            setErrorMessage("Введите ID задачи.");
            return;
        }

        try {
            clearMessages();

            const response = await API.get(`/tasks/${searchId}`);
            setFoundTask(response.data);
        } catch (error) {
            console.error("Ошибка поиска задачи:", error);
            setFoundTask(null);
            setErrorMessage("Задача с таким ID не найдена.");
        }
    };

    const startEditTask = (task) => {
        clearMessages();
        setEditingTaskId(task.id);
        setEditTitle(task.title);
        setEditDescription(task.description);
        setEditIsCompleted(task.isCompleted);
    };

    const cancelEditTask = () => {
        setEditingTaskId(null);
        setEditTitle("");
        setEditDescription("");
        setEditIsCompleted(false);
    };

    const handleUpdateTask = async (id) => {
        if (!editTitle.trim()) {
            setErrorMessage("Введите название задачи.");
            return;
        }

        try {
            clearMessages();

            await API.put(`/tasks/${id}`, {
                title: editTitle,
                description: editDescription,
                isCompleted: editIsCompleted
            });

            if (foundTask && foundTask.id === id) {
                setFoundTask({
                    ...foundTask,
                    title: editTitle,
                    description: editDescription,
                    isCompleted: editIsCompleted
                });
            }

            cancelEditTask();
            setSuccessMessage("Задача обновлена.");
        } catch (error) {
            console.error("Ошибка обновления задачи:", error);
            setErrorMessage("Не удалось обновить задачу.");
        }
    };

    const handleSendTaskEmail = async (id) => {
        try {
            clearMessages();

            const response = await API.post(`/mail/send-task/${id}`);
            setSuccessMessage(response.data.message || "Письмо отправлено.");
        } catch (error) {
            console.error("Ошибка отправки письма:", error);
            setErrorMessage("Не удалось отправить письмо.");
        }
    };

    const handleLoadImapMessages = async () => {
        try {
            clearMessages();

            const response = await API.get("/mail/imap/messages");
            setMailMessages(response.data);
            setMailSource("IMAP");
        } catch (error) {
            console.error("Ошибка чтения IMAP:", error);
            setErrorMessage("Не удалось получить письма по IMAP.");
        }
    };

    const handleLoadPop3Messages = async () => {
        try {
            clearMessages();

            const response = await API.get("/mail/pop3/messages");
            setMailMessages(response.data);
            setMailSource("POP3");
        } catch (error) {
            console.error("Ошибка чтения POP3:", error);
            setErrorMessage("Не удалось получить письма по POP3.");
        }
    };

    return (
        <div className="app-shell">
            <div className="app-container">
                <header className="hero">
                    <div>
                        <p className="eyebrow">Лабораторная работа</p>
                        <h1>ToDo List</h1>
                        <p className="hero-text">
                            Управление задачами и отправка по e-mail через SMTP, IMAP и POP3.
                        </p>
                        <p className="socket-status">Realtime: {socketStatus}</p>
                    </div>
                </header>

                {(errorMessage || successMessage) && (
                    <div className="messages">
                        {errorMessage && <div className="alert alert-error">{errorMessage}</div>}
                        {successMessage && <div className="alert alert-success">{successMessage}</div>}
                    </div>
                )}

                <div className="grid-top">
                    <section className="card">
                        <div className="card-header">
                            <h2>Добавить задачу</h2>
                            <p>Создание новой записи в списке задач.</p>
                        </div>

                        <div className="form-group">
                            <label>Название задачи</label>
                            <input
                                type="text"
                                placeholder="Например: Подготовить отчёт"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                            />
                        </div>

                        <div className="form-group">
                            <label>Описание</label>
                            <textarea
                                placeholder="Краткое описание задачи"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                            />
                        </div>

                        <button className="btn btn-primary" onClick={handleAddTask}>
                            Добавить задачу
                        </button>
                    </section>

                    <section className="card">
                        <div className="card-header">
                            <h2>Найти задачу по ID</h2>
                            <p>Детальный просмотр одной задачи.</p>
                        </div>

                        <div className="search-row">
                            <input
                                type="number"
                                placeholder="Введите ID"
                                value={searchId}
                                onChange={(e) => setSearchId(e.target.value)}
                            />
                            <button className="btn btn-dark" onClick={handleFindTaskById}>
                                Найти
                            </button>
                        </div>

                        {foundTask ? (
                            <div className="details-box">
                                <div className="details-row">
                                    <span>ID</span>
                                    <strong>{foundTask.id}</strong>
                                </div>
                                <div className="details-row">
                                    <span>Название</span>
                                    <strong>{foundTask.title}</strong>
                                </div>
                                <div className="details-row">
                                    <span>Описание</span>
                                    <strong>{foundTask.description || "—"}</strong>
                                </div>
                                <div className="details-row">
                                    <span>Статус</span>
                                    <strong>{foundTask.isCompleted ? "Выполнено" : "Не выполнено"}</strong>
                                </div>
                                <div className="details-row">
                                    <span>Создано</span>
                                    <strong>{new Date(foundTask.createdAt).toLocaleString()}</strong>
                                </div>
                            </div>
                        ) : (
                            <div className="placeholder-box">Здесь появится задача после поиска.</div>
                        )}
                    </section>
                </div>

                <section className="card">
                    <div className="card-header">
                        <h2>Почта</h2>
                        <p>Просмотр писем, полученных через IMAP и POP3.</p>
                    </div>

                    <div className="mail-toolbar">
                        <button className="btn btn-primary" onClick={handleLoadImapMessages}>
                            Показать письма IMAP
                        </button>
                        <button className="btn btn-secondary" onClick={handleLoadPop3Messages}>
                            Показать письма POP3
                        </button>
                    </div>

                    <div className="mail-panel">
                        <div className="mail-source">
                            Источник: <strong>{mailSource || "не выбран"}</strong>
                        </div>

                        {mailMessages.length === 0 ? (
                            <div className="placeholder-box">Писем пока нет.</div>
                        ) : (
                            <ul className="mail-list">
                                {mailMessages.map((message, index) => (
                                    <li key={index}>{message}</li>
                                ))}
                            </ul>
                        )}
                    </div>
                </section>

                <section className="card">
                    <div className="card-header">
                        <h2>Список задач</h2>
                        <p>Просмотр, редактирование, удаление и отправка по e-mail.</p>
                    </div>

                    {tasks.length === 0 ? (
                        <div className="placeholder-box">Задач пока нет.</div>
                    ) : (
                        <div className="table-wrap">
                            <table className="tasks-table">
                                <thead>
                                    <tr>
                                        <th>ID</th>
                                        <th>Название</th>
                                        <th>Описание</th>
                                        <th>Статус</th>
                                        <th>Дата создания</th>
                                        <th>Действия</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {tasks.map((task) => (
                                        <tr key={task.id}>
                                            <td>{task.id}</td>

                                            <td>
                                                {editingTaskId === task.id ? (
                                                    <input
                                                        type="text"
                                                        value={editTitle}
                                                        onChange={(e) => setEditTitle(e.target.value)}
                                                    />
                                                ) : (
                                                    task.title
                                                )}
                                            </td>

                                            <td>
                                                {editingTaskId === task.id ? (
                                                    <textarea
                                                        value={editDescription}
                                                        onChange={(e) => setEditDescription(e.target.value)}
                                                    />
                                                ) : (
                                                    task.description || "—"
                                                )}
                                            </td>

                                            <td>
                                                {editingTaskId === task.id ? (
                                                    <label className="checkbox-inline">
                                                        <input
                                                            type="checkbox"
                                                            checked={editIsCompleted}
                                                            onChange={(e) => setEditIsCompleted(e.target.checked)}
                                                        />
                                                        Выполнено
                                                    </label>
                                                ) : (
                                                    <span className={task.isCompleted ? "badge done" : "badge pending"}>
                                                        {task.isCompleted ? "Да" : "Нет"}
                                                    </span>
                                                )}
                                            </td>

                                            <td>{new Date(task.createdAt).toLocaleString()}</td>

                                            <td>
                                                {editingTaskId === task.id ? (
                                                    <div className="actions">
                                                        <button
                                                            className="btn btn-primary"
                                                            onClick={() => handleUpdateTask(task.id)}
                                                        >
                                                            Сохранить
                                                        </button>
                                                        <button className="btn btn-light" onClick={cancelEditTask}>
                                                            Отмена
                                                        </button>
                                                    </div>
                                                ) : (
                                                    <div className="actions">
                                                        <button
                                                            className="btn btn-light"
                                                            onClick={() => startEditTask(task)}
                                                        >
                                                            Редактировать
                                                        </button>
                                                        <button
                                                            className="btn btn-danger"
                                                            onClick={() => handleDeleteTask(task.id)}
                                                        >
                                                            Удалить
                                                        </button>
                                                        <button
                                                            className="btn btn-mail"
                                                            onClick={() => handleSendTaskEmail(task.id)}
                                                        >
                                                            Отправить e-mail
                                                        </button>
                                                    </div>
                                                )}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </section>
            </div>
        </div>
    );
}

export default App;