import axios from "axios";

export const axiosInstance = axios.create({
  // Use the exact URL you verified in the browser
  baseURL: "http://localhost:8080/api/Product", 
  headers: {
    "Content-Type": "application/json",
  },
});
