import { assets } from "../assets/assets";

export default function StarRating({ rating = 4 }: { rating?: number }) {
  return (
    <>
      {Array(5)
        .fill(0)
        .map((_, index) => (
          <img
            key={index}
            src={
              rating > index ? assets.starIconFilled : assets.starIconOutlined
            }
            alt="star-icon"
            className="w-4.5 h-4.5"
          />
        ))}
    </>
  );
}
