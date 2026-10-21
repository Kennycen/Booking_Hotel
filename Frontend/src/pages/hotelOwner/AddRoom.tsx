import { useState, type ChangeEvent, type SubmitEvent } from "react";
import toast from "react-hot-toast";
import { assets } from "../../assets/assets";
import Title from "../../components/Title";
import { useAppContext } from "../../context/AppContext";
import { errorMessage } from "../../lib/errorMessage";
import type { ApiResponse } from "../../types";

const amenityNames = [
  "Free WiFi",
  "Free Breakfast",
  "Room Service",
  "Mountain View",
  "Pool Access",
] as const;

const imageSlots = ["1", "2", "3", "4"] as const;
const roomTypes = ["Single Bed", "Double Bed", "Luxury Room", "Family Suite"];

type AmenityName = (typeof amenityNames)[number];
type ImageSlot = (typeof imageSlots)[number];

type RoomInputs = {
  roomType: string;
  pricePerNight: number;
  amenities: Record<AmenityName, boolean>;
};

function emptyAmenities(): Record<AmenityName, boolean> {
  return {
    "Free WiFi": false,
    "Free Breakfast": false,
    "Room Service": false,
    "Mountain View": false,
    "Pool Access": false,
  };
}

type SelectedImage = {
  file: File;
  preview: string;
};

function emptyImages(): Record<ImageSlot, SelectedImage | null> {
  return { "1": null, "2": null, "3": null, "4": null };
}

function revokeImages(images: Record<ImageSlot, SelectedImage | null>) {
  imageSlots.forEach((slot) => {
    const image = images[slot];
    if (image) URL.revokeObjectURL(image.preview);
  });
}

type RoomImageFieldProps = {
  slot: ImageSlot;
  preview: string | null;
  onSelect: (slot: ImageSlot, file: File | null) => void;
};

function RoomImageField({ slot, preview, onSelect }: RoomImageFieldProps) {
  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    onSelect(slot, event.target.files?.[0] ?? null);
  };

  return (
    <label htmlFor={`roomImage${slot}`}>
      <img
        className="max-h-13 cursor-pointer opacity-80"
        src={preview ?? assets.uploadArea}
        alt=""
      />
      <input
        type="file"
        accept="image/*"
        id={`roomImage${slot}`}
        hidden
        onChange={handleChange}
      />
    </label>
  );
}

export default function AddRoom() {
  const { axios, getToken } = useAppContext();
  const [images, setImages] =
    useState<Record<ImageSlot, SelectedImage | null>>(emptyImages);
  const [inputs, setInputs] = useState<RoomInputs>({
    roomType: "",
    pricePerNight: 0,
    amenities: emptyAmenities(),
  });
  const [loading, setLoading] = useState(false);

  const onSubmitHandler = async (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault();
    const selectedImages = imageSlots.flatMap((slot) => {
      const image = images[slot];
      return image ? [image.file] : [];
    });

    if (
      !inputs.roomType ||
      !inputs.pricePerNight ||
      selectedImages.length === 0
    ) {
      toast.error("Please fill in all the details");
      return;
    }

    setLoading(true);
    try {
      const formData = new FormData();
      formData.append("roomType", inputs.roomType);
      formData.append("pricePerNight", String(inputs.pricePerNight));
      const amenities = amenityNames.filter((name) => inputs.amenities[name]);
      formData.append("amenities", JSON.stringify(amenities));
      selectedImages.forEach((image) => formData.append("images", image));

      const { data } = await axios.post<ApiResponse>("/api/rooms/", formData, {
        headers: { Authorization: `Bearer ${await getToken()}` },
      });

      if (data.success) {
        toast.success(data.message ?? "Room created successfully");
        setInputs({
          roomType: "",
          pricePerNight: 0,
          amenities: emptyAmenities(),
        });
        revokeImages(images);
        setImages(emptyImages());
      } else {
        toast.error(data.message ?? "Could not add room");
      }
    } catch (error) {
      toast.error(errorMessage(error, "Could not add room"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={onSubmitHandler}>
      <Title
        align="left"
        font="outfit"
        title="Add Room"
        subTitle="Fill in the details carefully and accurate room details, pricing, and amenities, to enhance the user booking experience."
      />
      <p className="text-gray-800 mt-10">Images</p>
      <div className="grid grid-cols-2 sm:flex gap-4 my-2 flex-wrap">
        {imageSlots.map((slot) => (
          <RoomImageField
            key={slot}
            slot={slot}
            preview={images[slot]?.preview ?? null}
            onSelect={(selectedSlot, file) => {
              const previous = images[selectedSlot];
              if (previous) URL.revokeObjectURL(previous.preview);
              setImages({
                ...images,
                [selectedSlot]: file
                  ? { file, preview: URL.createObjectURL(file) }
                  : null,
              });
            }}
          />
        ))}
      </div>

      <div className="w-full flex max-sm:flex-col sm:gap-4 mt-4">
        <div className="flex-1 max-w-48">
          <p className="text-gray-800 mt-4">Room Type</p>
          <select
            value={inputs.roomType}
            onChange={(event) =>
              setInputs({ ...inputs, roomType: event.target.value })
            }
            className="border opacity-70 border-gray-300 mt-1 rounded p-2 w-full"
          >
            <option value="">Select Room Type</option>
            {roomTypes.map((roomType) => (
              <option key={roomType} value={roomType}>
                {roomType}
              </option>
            ))}
          </select>
        </div>
        <div>
          <p className="mt-4 text-gray-800">
            Price <span className="text-xs">/night</span>
          </p>
          <input
            type="number"
            placeholder="0"
            value={inputs.pricePerNight || ""}
            className="border border-gray-300 mt-1 rounded p-2 w-24"
            onChange={(event) =>
              setInputs({
                ...inputs,
                pricePerNight: Number(event.target.value),
              })
            }
          />
        </div>
      </div>

      <p className="text-gray-800 mt-4">Amenities</p>
      <div className="flex flex-col flex-wrap mt-1 text-gray-400 max-w-sm">
        {amenityNames.map((amenity, index) => (
          <div key={amenity}>
            <input
              type="checkbox"
              id={`amenities${index + 1}`}
              checked={inputs.amenities[amenity]}
              onChange={() =>
                setInputs({
                  ...inputs,
                  amenities: {
                    ...inputs.amenities,
                    [amenity]: !inputs.amenities[amenity],
                  },
                })
              }
            />
            <label htmlFor={`amenities${index + 1}`}> {amenity}</label>
          </div>
        ))}
      </div>
      <button
        className="bg-primary text-white px-8 py-2 rounded mt-8 cursor-pointer"
        disabled={loading}
      >
        {loading ? "Adding..." : "Add Room"}
      </button>
    </form>
  );
}
