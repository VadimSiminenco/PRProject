import axios from "axios";

export const API_BASE = "http://localhost:8080";

const API = axios.create({
    baseURL: `${API_BASE}/api`
});

export default API;