import { useEffect } from "react";
import { Outlet } from "react-router-dom";
import Navbar from "../../components/hotelOwner/Navbar";
import Sidebar from "../../components/hotelOwner/Sidebar";
import { useAppContext } from "../../context/AppContext";

export default function Layout() {
  const { isOwner, navigate, authLoaded, userReady } = useAppContext();

  useEffect(() => {
    if (!authLoaded || !userReady) return;
    if (!isOwner) navigate("/");
  }, [authLoaded, isOwner, navigate, userReady]);

  if (!authLoaded || !userReady || !isOwner) return null;

  return (
    <div className="flex flex-col h-screen">
      <Navbar />
      <div className="flex h-full">
        <Sidebar />
        <div className="flex-1 p-4 pt-10 md:px-10 h-full">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
